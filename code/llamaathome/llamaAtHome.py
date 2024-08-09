# Ryker Zierden
# Grand Canyon University
# The chatbot portions of this script were started off with exllamav2's minimal chatbot example script

import requests
import sys
import os
parent_dir = os.path.dirname(sys.path[0])
sys.path.append(parent_dir + "\\exllamav2")
from exllamav2 import *
from exllamav2.generator import *
import torch
import json
import yaml
import random

# llamaAtHome config options
debug_messages = False
reset_after_prompt = False
simulate_api_communication = False

class ModelManager:
    def __init__(self, training_inputs_path, generate_request_bodies):
        self.training_inputs_path = training_inputs_path
        self.training_inputs = []
        self.generate_request_bodies = generate_request_bodies
        # read in training prompts
        self.read_training_inputs(training_inputs_path)
        
        self.initialize()
    
    # initializes model. Can be used to reset model to original state
    def initialize(self):
        if(debug_messages):
            print("Starting model initialization...")
         # initialize the exllama model
        self.config = ExLlamaV2Config()
        self.config.model_dir = parent_dir  + "\\exllamav2\\Llama2-7B-chat-exl2"

        self.config.prepare()
        self.config.max_seq_len = 8000
        
        self.model = ExLlamaV2(self.config)
        self.cache = ExLlamaV2Cache(self.model, lazy = True)

        # set the domains that we care about
        self.domains = {"light"}
        
        if(debug_messages):
            print("Reading states and services...")
        # load in available states and services
        self.states = open("states.json").read()
        self.statesJson = json.loads(self.states)
        
        self.services = open("services.json").read()
        self.servicesJson = json.loads(self.services)
        self.service_information = dict()
        self.entities = []
        for state in self.statesJson:
            entity_id = state["entity_id"]
            if(str(entity_id).split(".")[0].startswith("light")):
                self.entities.append(entity_id)
                        
        self.tokenizer = ExLlamaV2Tokenizer(self.config)
        self.generator = ExLlamaV2StreamingGenerator(self.model, self.cache, self.tokenizer)
        
        self.gen_settings = ExLlamaV2Sampler.Settings()
        
        
        self.instructions_log = []
        self.responses_log = []
        
        
        self.generator.set_stop_conditions([self.tokenizer.eos_token_id])
        self.model.load_autosplit(self.cache)
        if(debug_messages):
            print("Model finished loading! Starting training services...")   
        # send each state in json format to the model to train it up on what's available
        self.send_instruction("The following information represents the available home assistant service names and json formatted descriptions in the following format:\nSERVICE_NAME: <service_name>\nSERVICE_INFORMATION: <json formatted service information>", False)
        for service_domain in self.servicesJson:
            if(service_domain["domain"] == "light"):
                for service_name in service_domain["services"]:
                    service_info = service_domain["services"][service_name]
                    self.service_information[service_name] = service_info
                    instruction = ""
                    instruction += "SERVICE_NAME: " + service_name + "\n"
                    instruction += "SERVICE_INFORMATION: " + json.dumps(service_info) + "\n"
                    self.send_instruction(instruction,False)
            
        if(debug_messages):
            print("Loading training inputs...")
        # send each training input to the model sequentially. There may be a more efficient or effective way to do this in the future
        for training_input in self.training_inputs:
            self.send_instruction(training_input,False)
            
    # send instruction to ExLlamaV2, meant to be used mostly internally. The workings of this function are borrowed heavily from "minimal_chat.py", provided as an example of how to setup a chatbot in ExLlamaV2 (https://github.com/turboderp/exllamav2/blob/master/examples/minimal_chat.py)
    def send_instruction(self, input, generate_response = True,start_filter=""):
        instruction_ids = self.tokenizer.encode(f"[INST]" + input + "[/INST]")
        context_ids = instruction_ids if self.generator.sequence_ids is None \
        else torch.cat([self.generator.sequence_ids, instruction_ids], dim = -1)
        self.instructions_log.append(input)
        if(debug_messages):
            print("MODEL INPUT: " + input)
        if(generate_response):
            settings = self.gen_settings
            #if(start_filter != ""):
                #settings.begin_filters(start_filter)
            self.generator.begin_stream(context_ids, settings)
            eos = False
            response = ""
            while True:
                response_chunk, eos, _ = self.generator.stream()
                if(eos):
                    break
                else:
                    response += response_chunk
            self.responses_log.append(response)
            
            if(debug_messages):
                print("MODEL RESPONSE: " + response)
            if(start_filter != "" and start_filter in response):
                response = response.split(start_filter)[1]
                if(debug_messages):
                    print("TRIMMED RESPONSE: " + response)
            return response
        return None
    def generate_single_word(self,input,start_filter=""):
        instruction_ids = self.tokenizer.encode(f"[INST]" + input + "[/INST]")
        context_ids = instruction_ids if self.generator.sequence_ids is None \
        else torch.cat([self.generator.sequence_ids, instruction_ids], dim = -1)
        self.instructions_log.append(input)
        if(debug_messages):
            print("MODEL INPUT: " + input)
        settings = self.gen_settings
        #if(start_filter != ""):
            #settings.begin_filters(start_filter)
        self.generator.begin_stream(context_ids, settings)
        result = ""
        eos = False
        filter_found = False
        while not eos:
            word = ""
            while(' ' not in word.lstrip(' ').replace(": ","") and not eos):
                result,eos,_ = self.generator.stream()
                word += result
            if(filter_found and (word != "" and word != " ")):
                break
            if(start_filter.strip(':').strip(' ') in word):
                filter_found = True
                if(start_filter in word and len(word.strip()) > len(start_filter.strip())):
                    word = word.replace(start_filter,"").replace("\n","").replace(":","").replace(" ","")
                    break
        if(debug_messages):
            print("MODEL RESPONSE: " + word)
        return word
    
    # reads training inputs from a text file into the list of training inputs
    def read_training_inputs(self,path):
        file = open(path)
        self.training_inputs = file.readlines()
    # calls the initialize function again to reset model to original state. Needs to retrain training inputs. 
    def reset_model_context(self):
        self.model.unload()
        self.initialize()
    def generate_api_info(self,input):
        # send the instruction with formatting information
        entity_name_with_domain = self.generate_single_word("For this user input: \"" + input + "\" choose a home assistant entity from the following list of available entities: " + json.dumps(self.entities) + ". Provide this choice in the following format:\nENTITY:<chosen_entity_id>","ENTITY:")
        
        service_name = self.generate_single_word("For this user input: \"" + input + "\" choose a home assistant service from the following list of available services: " + json.dumps(list(self.service_information.keys())) + ". Provide this choice in the following format:\nSERVICE_NAME:<chosen_service_name>","SERVICE_NAME:")
        
        # parse entity domain and name from chatbot response
        entity_domain,entity_name = entity_name_with_domain.split('.')
        
        request_body = dict()
        if(self.generate_request_bodies): 
            field_list = list(self.service_information[service_name]["fields"].keys())
            request_body_response = self.send_instruction("For this user input: \"" + input + "\" create the request data in json format for a request for the service " + service_name + ". The following fields are available to put into the request body, but most of them are unnecessary for this request: " + str(json.dumps(field_list)) + ".\nPlease provide your answer in the following format:\nREQUEST_BODY:<json request body>\nMake sure to use correct JSON formatting in the response",True,"REQUEST_BODY:")
            # parse request body from chatbot response
            request_body_start = request_body_response.find("{")
            request_body_end = request_body_response.rfind("}",request_body_start) + 1
            request_body = request_body_response[request_body_start:request_body_end]
            try:
                request_body = json.loads(request_body)
                # check the JSON information about the services and see if the option and keys are valid for the request we're making
                for item in list(request_body.keys()):
                    if item not in field_list:
                        request_body.pop(item)
                    else:
                        if("selector" in self.service_information[service_name]["fields"][item].keys() and "select" in self.service_information[service_name]["fields"][item]["selector"]):
                            options = self.service_information[service_name]["fields"][item]["selector"]["select"]["options"]
                            found_option = False
                            for option in options:
                                if(str(request_body[item]) in str(option)):
                                    found_option = True
                                    break
                            if(not found_option):
                                request_body.pop(item)
            except:
                if(debug_messages):
                    print("Failed to parse JSON and remove invalid API options: " + str(request_body) + "!") 
        # the llm doesn't always add this data, but it's usually required for lights and switches
        if(entity_domain == "light" or entity_domain == "switch"):
            try:
                request_body["entity_id"] = entity_name_with_domain
            except:
                if(debug_messages):
                    print("Failed insert entity_id in JSON: " + str(request_body) + "!!") 
        return entity_domain,service_name,request_body
    def generate_user_response_api(self,api_response):
        user_response = self.send_instruction("The API request was sent to home assistant and received the following result: \"" + str(api_response) + "\"\nPlease generate a response for the user of the program in the following format that does not mention the API result:\nUSER_RESPONSE:<user response>\nCodes in the 400s typically indicate a failure.",True,"USER_RESPONSE:")
        return user_response
    def generate_user_response(self,input):
        user_response = self.send_instruction("Please generate a friendly user response for this input: " + str(input) + "\nProvide this response in the following format:\nUSER_RESPONSE:<user response>",True,"USER_RESPONSE:")
        return user_response
    def check_for_api_requirement(self,input):
        llama_response = self.generate_single_word("The following input was provided by the user: " + str(input) + "\nPlease determine whether or not this input relates to changing something in user's the smart home. Please answer \"true\" if the request relates to the smart home or smart devices and \"false\" if it does not. Provide your response in the following format:\nREQUIRES_API:<true or false>","REQUIRES_API:")
        if("true" in llama_response.lower()):
            return True
        else:
            return False
    
# simple class to send API requests to home assistant
class HomeAssistantDelegate:
    def __init__(self,base_url,api_key):
        self.base_url = str(base_url).rstrip('/')
        self.services_url = base_url + "/api/services"
        self.api_key = api_key
        self.common_headers = headers = {"Authorization":"Bearer " + self.api_key, "content-type" : "application/json"}
    # sends an API request to home assistant and returns the response as headers and a body
    def send_post_request(self,service_name,entity_domain,request_body):
        url = self.services_url + "/" + entity_domain + "/" + service_name
        headers = self.common_headers
        api_response = requests.post(url,headers=headers,json=request_body)
        return api_response
    # updates states.json and services.json
    def update_states_and_services(self):
        f = open("services.json","w")
        f.write((requests.get(self.services_url, headers=self.common_headers).text))
        f = open("states.json","w")
        f.write((requests.get(self.base_url + "/api/states", headers=self.common_headers).text))
# this class is basically nothing right now, but will allow for a web UI to be seamlessly integrated in the future        
class IOManager:
    def __init__(self):
        pass
    def output(self, message):
        print(message)
    def get_input(self,prompt = ""):
        return input(prompt)
        
def read_config(config_file_path):
    yaml_config = yaml.safe_load(open(config_file_path))
    llama_config = yaml_config["llama_at_home"]
    
    return llama_config
# helper function with main loop to help with testing, essentially the same as "main" on the system diagram
def main_helper(user_input,home_assistant_delegate,model_manager,io_manager,debug_messages,reset_after_prompt,simulate_api_communication):
    requires_api_request = model_manager.check_for_api_requirement(user_input)
    user_response = ""
    if(requires_api_request):
        api_info = model_manager.generate_api_info(user_input)
        if(debug_messages):
            print("API info generated by model: " + str(api_info))
        entity_domain,service_name,request_body = api_info
        if(simulate_api_communication):
            api_response = random.choice(["SUCCESS","ERROR"])
            if(debug_messages):
                print("Random API response generated")
        else:
            api_response = home_assistant_delegate.send_post_request(service_name,entity_domain,request_body)
        if(debug_messages):
            print("API Response: " + str(api_response))
        user_response = model_manager.generate_user_response_api(api_response)
    else:
        user_response = model_manager.generate_user_response(user_input)
    io_manager.output("Llama at Home: " + user_response)
    if(reset_after_prompt):
        model_manager.reset_model_context()
        
def main():
    config = read_config("config-ryker.yaml")
    if "debug_messages" in config:
        debug_messages = bool(config["debug_messages"])
    if "reset_after_prompt" in config:
        reset_after_prompt = bool(config["reset_after_prompt"])
    if "simulate_api_communication" in config:
        simulate_api_communication = bool(config["simulate_api_communication"])
        
    if(debug_messages):    
        print("Starting llama_at_home...")    
        
    home_assistant_delegate = HomeAssistantDelegate(config["url"],config["api_key"])
    model_manager = ModelManager(config["training_inputs_path"],config["generate_request_bodies"])
    io_manager = IOManager()
    while True:
        user_input = io_manager.get_input("Enter a prompt for Llama at Home:\n")
        main_helper(user_input,home_assistant_delegate,model_manager,io_manager,debug_messages,reset_after_prompt,simulate_api_communication)   


if __name__ == "__main__":
    main()
    

            
        
        