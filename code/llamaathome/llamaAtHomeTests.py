# Ryker Zierden
# Llama at Home Tests
# Grand Canyone University: CSET
# Dr. Aiman Darwich

import llamaAtHome
import pandas as pd

wait_for_user_checks = True
run_component_tests = False
run_requirements_tests = False
run_system_tests = True

ryker_config = llamaAtHome.read_config("config-ryker.yaml")
home_assistant_delegate = llamaAtHome.HomeAssistantDelegate(ryker_config["url"],ryker_config["api_key"])
io_manager = llamaAtHome.IOManager()
model_manager = llamaAtHome.ModelManager("training_inputs.txt",False)
 
if(run_component_tests):
    # Component Tests
    print("##### COMPONENT TESTS #####")
    print("### Test 1a ###")
    config = llamaAtHome.read_config("config.yaml")
    print("config contents:\n" + str(config))
    print("### Test 2a ###")
    print("type of object: " + str(type(io_manager)))
    print("### Test 2b ###")
    print("test output (expected: TEST): ")
    io_manager.output("TEST")
    print("### Test 2c ###")
    received_input = ""
    if wait_for_user_checks:
        print("provide an input: ")
        received_input = io_manager.get_input()
        print("received input: " + received_input)
    print("### Test 2d ###")
    received_input = ""
    if wait_for_user_checks:
        received_input = io_manager.get_input("INPUT_HERE: ")
        print("Expected prior line: INPUT_HERE:\nreceived input: " + received_input)
    print("### Test 3a ###")
    ryker_config = llamaAtHome.read_config("config-ryker.yaml")
    home_assistant_delegate = llamaAtHome.HomeAssistantDelegate(ryker_config["url"],ryker_config["api_key"])
    print("type of object: " + str(type(home_assistant_delegate)))
    print("### Test 3b ###")
    result = home_assistant_delegate.send_post_request("turn_on","light",dict({"entity_id": "light.ryker_desk_lights"}))
    print(result)
    if(wait_for_user_checks):
        print("test administrator response: " + io_manager.get_input("did the desk lights turn on?: "))
    print("### Test 3c ###")
    home_assistant_delegate.update_states_and_services()
    print("states.json length: " + str(len(open("states.json").read())))
    print("services.json length: " + str(len(open("services.json").read())))
    print("### Test 4a ###")
    print("type of object: " + str(type(model_manager)))
    print("### Test 4b ###")
    model_manager.read_training_inputs("training_inputs.txt")
    print("training inputs array:\n" + str(model_manager.training_inputs))
    print("### Test 4c ###")
    response = model_manager.send_instruction("What's the capital of Japan?",False)
    print("reponse type (expected NoneType): " + str(type(response)))
    response = model_manager.send_instruction("What's the capital of Japan?",True)
    print("response: " + response)
    response = model_manager.send_instruction("What's the capital of Japan?",True,"Tokyo")
    print("response (expected mid-sentence cut): " + response)
    print("### Test 4d ###")
    bool_response = model_manager.check_for_api_requirement("turn on the bedroom lights")
    print("response (expect True or 1): " + str(bool_response))
    print("### Test 4e ###")
    response = model_manager.generate_single_word("Tell me the first name of the first president of the United States in the following format: FIRST_NAME:<name>","FIRST_NAME")
    print("response (expect one word): " + response)
    print("### Test 4f ###")
    response = model_manager.generate_api_info("turn on Ryker's desk lights")
    print("api response: " + str(response))
    print("### Test 4g ###")
    response = model_manager.generate_user_response("What's the size of the moon?")
    print("user response: " + response)
    print("### Test 4h ###")
    response = model_manager.generate_user_response_api("404: Not Found")
    print("API user response (expect failed message): " + response)
    print("### Test 4i ###")
    model_manager.send_instruction("My name is Ryker",False)
    response_with_context = model_manager.send_instruction("What's my name?")
    model_manager.reset_model_context()
    response_no_context = model_manager.send_instruction("What's my name?")
    print("With context: " + response_with_context)
    print("Without context: " + response_no_context)
# Requirements Tests
if(run_requirements_tests):
    print("\n\n\n##### REQUIREMENTS TESTS #####")
    model_manager = llamaAtHome.ModelManager("training_inputs.txt",False)
    
    tpt = pd.read_csv("TestPrompts.csv")
    for ri in range(tpt.count()[0]):
        print("\n### Test " + str(tpt["Test ID"][ri]) + " ###")
        print("prompt: " + str(tpt["Prompt"][ri]))
        print("category: " + str(tpt["Test Category"][ri]))
        try:
            this_response = llamaAtHome.main_helper(str(tpt["Prompt"].iloc[ri]),home_assistant_delegate,model_manager,io_manager,True,True,False)
            if(wait_for_user_checks):
                print("test administrator response: " + io_manager.get_input("Was the test a success?"))
        except Exception as ex:
            print("TEST FAILED: " + str(ex))

# System Tests
if(run_system_tests):
    print("\n\n\n##### SYSTEM TESTS #####")
    print("### 1a ###")
    print("before: " + model_manager.send_instruction("Do you know what my name is?"))
    fs = open("test_training_inputs.txt","w+")
    fs.writelines(["My name is Ryker","Please give as short of answers as possible"])
    fs.close()
    model_manager.read_training_inputs("test_training_inputs.txt")
    model_manager.reset_model_context()
    print("after: " + model_manager.send_instruction("Do you know what my name is?"))
    
    print("### 2a, 2b, 2c ###")
    fs = open("config_test.yaml","w+")
    fs.writelines(["---\n","llama_at_home:\n","  debug_messages: true\n","  reset_after_prompt: true\n", "  simulate_api_communication: true\n"])
    fs.close()
    test_config = llamaAtHome.read_config("config_test.yaml")
    print("2a) debug messages (expect True or 1): " + str(test_config["debug_messages"]))
    print("2b) reset after prompt (expect True or 1): " + str(test_config["debug_messages"]))
    print("2c) simulate api communication (expect True or 1): " + str(test_config["debug_messages"]))
    print("Test prompt with all options on:")
    llamaAtHome.main_helper("My name is Ryker, turn on the lights in the bedroom",home_assistant_delegate,model_manager,io_manager,True,True,True)
    print("context check: " + model_manager.send_instruction("What's my name?"))
    print("3a: Covered by previous tests")
    print("4a: CLI starting...\n")
    if(wait_for_user_checks):
        llamaAtHome.main()
        