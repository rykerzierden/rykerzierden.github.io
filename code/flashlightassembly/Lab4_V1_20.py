# Written by Yuha Yoo & Ryker Zierden
# This macro shows an example of running a program on the robot using the Python API (online programming)
# More information about the RoboDK API appears here:
# https://robodk.com/doc/en/RoboDK-API.html
from robolink import *    # API to communicate with RoboDK
from robodk import *      # robodk robotics toolbox
import numpy 
# Any interaction with RoboDK must be done through RDK:
RDK = Robolink()
# Select a robot (a popup is displayed if more than one robot is available)
robot = RDK.ItemUserPick('Select a robot', ITEM_TYPE_ROBOT)
if not robot.Valid():
    raise Exception('No robot selected or available')
RUN_ON_ROBOT = False
# Important: by default, the run mode is RUNMODE_SIMULATE
# If the program is generated offline manually the runmode will be RUNMODE_MAKE_ROBOTPROG,
# Therefore, we should not run the program on the robot
if RDK.RunMode() != RUNMODE_SIMULATE:
    RUN_ON_ROBOT = False
if RUN_ON_ROBOT:

    # Connect to the robot using default IP
    success = robot.Connect() # Try to connect once
    status, status_msg = robot.ConnectedState()
    if status != ROBOTCOM_READY:
        # Stop if the connection did not succeed
        print(status_msg)
        raise Exception("Failed to connect: " + status_msg)
    # This will set to run the API programs on the robot and the simulator (online programming)
    RDK.setRunMode(RUNMODE_RUN_ROBOT)


# Get the current joint position of the robot
# (updates the position on the robot simulator)
joints_ref = robot.Joints().list()
# get the current position of the TCP with respect to the reference frame:
# (4x4 matrix representing position and orientation)
target_ref = robot.Pose()
pos_ref = target_ref.Pos()
# It is important to provide the reference frame and the tool frames when generating programs offline
# It is important to update the TCP on the robot mostly when using the driver
robot.setPoseFrame(robot.PoseFrame())
robot.setPoseTool(robot.PoseTool())
#robot.setZoneData(10) # Set the rounding parameter (Also known as: CNT, APO/C_DIS, ZoneData, Blending radius, cornering, ...)
robot.setSpeed(200) # Set linear speed in mm/s

# Setting Pose 1, Pose 2, and Pose 3 from the Lab Manual
# We enter our xyzrpw (X, Y, Z, RX, RY, RZ) into the function xyzrpw_2_pose()
# to convert out position and euler angles to a 4x4 pose matrix
# Start at the home position
home = [0, -90, 0, -90, 0, 0]
intermediate = [14.66,-105.50,108.30,-91.78,-88.23,-167.49]
# intermediate = [54,-113,89,-90,-87,87]
# define starting rotation and position of each pallet
pallet_xstart = -363.78
pallet_ystart = -90.45
pallet_rotation = math.radians(-90)


clamp_x = -332.11
clamp_y = 36.88

def tighten_custom(num_turns):
    whole_turns = round(num_turns)
    partial_turns = num_turns - whole_turns
    robot.setAccelerationJoints(360)
    for i in range(whole_turns):
        turn()
        robot.RunCodeCustom('rq_open_and_wait()',INSTRUCTION_CALL_PROGRAM)
        unturn()
        robot.RunCodeCustom('rq_close_and_wait()',INSTRUCTION_CALL_PROGRAM)
    robot.setAccelerationJoints(100)
    
    # currentJointValues = robot.Joints().list()
    # newJointValues = currentJointValues
    # newJointValues[5] = currentJointValues[5] + 360*partial_turns
    # robot.MoveJ(newJointValues)



    return robot.Joints().list()

def turn():
        currentJointValues = robot.Joints().list()
        newJointValues = currentJointValues
        newJointValues[5] = currentJointValues[5] + 359
        robot.MoveJ(newJointValues)

def unturn():
        currentJointValues = robot.Joints().list()
        newJointValues = currentJointValues
        newJointValues[5] = currentJointValues[5] - 359
        robot.MoveJ(newJointValues)



def transform_coordinates(rotation, xlocation, ylocation): 
    x = numpy.array([50, 125, 200, 280])
    y = numpy.array([35,35,35,35])
    

    #x = numpy.multiply(x,25.4)
    #y = numpy.multiply(y,25.4)
    
    radii = numpy.sqrt(numpy.square(x) + numpy.square(y))
    theta = numpy.arctan2(y,x)

    theta = numpy.add(theta, rotation)

    x1 = numpy.multiply(numpy.cos(theta), radii)
    y1 = numpy.multiply(numpy.sin(theta), radii)

    x1 = numpy.add(xlocation, x1)
    y1 = numpy.add(ylocation, y1)
    return (x1,y1)

# constants
z_clamp_head = 170 #mm
z_parts = 65 #mm -- 60mm is lowest
z_components_pick = [65, 75.9, 65.6, 62]
z_components_place = [203.4, 255, 252, 170]
barrel = 0
battery = 1
tail = 2
head = 3

# set initial joint speed and acceleration
robot.setAccelerationJoints(80)
robot.setSpeedJoints(180)
robot.RunCodeCustom('rq_set_force(255)',INSTRUCTION_CALL_PROGRAM)
robot.RunCodeCustom('rq_set_speed(255)',INSTRUCTION_CALL_PROGRAM)

# find coordinates
pallet_x,pallet_y = transform_coordinates(pallet_rotation,pallet_xstart,pallet_ystart)

robot.RunCodeCustom('unclamp()', INSTRUCTION_INSERT_CODE)
robot.MoveJ(home)
robot.MoveJ(intermediate)

#moving head to clamp
robot.MoveJ(xyzrpw_2_pose([pallet_x[head],pallet_y[head], z_components_pick[head] + 50, 0, 180, 0]))
robot.MoveL(xyzrpw_2_pose([pallet_x[head],pallet_y[head], z_components_pick[head], 0, 180, 0]))
robot.RunCodeCustom('rq_close_and_wait()',INSTRUCTION_CALL_PROGRAM)
robot.MoveL(xyzrpw_2_pose([pallet_x[head],pallet_y[head], z_components_place[head], 0, 180, 0]))
robot.MoveL(xyzrpw_2_pose([clamp_x,clamp_y, z_components_place[head] + 50, 0, 180, 0]))
robot.MoveL(xyzrpw_2_pose([clamp_x,clamp_y, z_components_place[head], 0, 180, 0]))
robot.RunCodeCustom('rq_open_and_wait()',INSTRUCTION_CALL_PROGRAM)
robot.RunCodeCustom('clamp()', INSTRUCTION_INSERT_CODE)
robot.RunCodeCustom('rq_open_and_wait()',INSTRUCTION_CALL_PROGRAM)
robot.MoveL(xyzrpw_2_pose([clamp_x,clamp_y, 400, 0, 180, 0]))


#moving barrel to head and twisting on 
robot.MoveJ(xyzrpw_2_pose([pallet_x[barrel],pallet_y[barrel],z_components_pick[barrel]+50.0,0,180,0]))
robot.MoveL(xyzrpw_2_pose([pallet_x[barrel],pallet_y[barrel],z_components_pick[barrel],0,180,0]))
robot.RunCodeCustom('rq_close_and_wait()',INSTRUCTION_CALL_PROGRAM)
robot.MoveL(xyzrpw_2_pose([pallet_x[barrel],pallet_y[barrel],z_components_place[barrel]+ 50,0,180,0]))
robot.MoveL(xyzrpw_2_pose([clamp_x,clamp_y,z_components_place[barrel] + 50,0,180,0]))
robot.MoveL(xyzrpw_2_pose([clamp_x,clamp_y,z_components_place[barrel],0,180,0]))
robot.RunCodeCustom('rq_open_and_wait()',INSTRUCTION_CALL_PROGRAM)
robot.RunCodeCustom('rq_close_and_wait()',INSTRUCTION_CALL_PROGRAM)
robot.MoveL(xyzrpw_2_pose([clamp_x,clamp_y,z_components_place[barrel]-1.0,0,180,0]))
tighten_custom(5)
robot.RunCodeCustom('tighten_torque(3, 0, 1.57, 2,2,1,100,100,50)',INSTRUCTION_INSERT_CODE)
robot.MoveL(xyzrpw_2_pose([clamp_x,clamp_y, 400, 0, 180, 0]))

#moving battery into barrel 
robot.MoveJ(xyzrpw_2_pose([pallet_x[battery], pallet_y[battery], z_components_pick[battery] + 75, 0, 180, 0]))
robot.MoveL(xyzrpw_2_pose([pallet_x[battery], pallet_y[battery], z_components_pick[battery], 0, 180, 0]))
robot.RunCodeCustom('rq_close_and_wait()',INSTRUCTION_CALL_PROGRAM)
robot.MoveL(xyzrpw_2_pose([pallet_x[battery], pallet_y[battery], z_components_place[battery] + 75, 0, 180, 0]))
robot.MoveL(xyzrpw_2_pose([clamp_x+1,clamp_y-1,z_components_place[battery]+100.0,0,180,0]))
robot.MoveL(xyzrpw_2_pose([clamp_x+1,clamp_y-1,z_components_place[battery],0,180,0]))
robot.RunCodeCustom('rq_open_and_wait()', INSTRUCTION_CALL_PROGRAM)
robot.MoveL(xyzrpw_2_pose([clamp_x,clamp_y, 400, 0, 180, 0]))

#moving tail piece to barrel and twisting on
robot.MoveJ(xyzrpw_2_pose([pallet_x[tail], pallet_y[tail], z_components_pick[tail] + 50, 0, 180, 0]))
robot.MoveL(xyzrpw_2_pose([pallet_x[tail], pallet_y[tail], z_components_pick[tail], 0, 180, 0]))
robot.RunCodeCustom('rq_set_force(100)',INSTRUCTION_CALL_PROGRAM)
robot.RunCodeCustom('rq_close_and_wait()',INSTRUCTION_CALL_PROGRAM)
robot.MoveL(xyzrpw_2_pose([pallet_x[tail], pallet_y[tail], z_components_place[tail] + 100, 0, 180, 0]))
robot.MoveL(xyzrpw_2_pose([clamp_x,clamp_y,z_components_place[tail]+60.0,0,180,0]))
robot.MoveL(xyzrpw_2_pose([clamp_x,clamp_y,z_components_place[tail],0,180,0]))
robot.RunCodeCustom('rq_set_force(255)',INSTRUCTION_CALL_PROGRAM)
tighten_custom(3)
robot.RunCodeCustom('tighten_torque(3,0,1.57,2,2,1,100,100,50)',INSTRUCTION_INSERT_CODE)

#moving finished product back to head position 
robot.RunCodeCustom('rq_open_and_wait()', INSTRUCTION_CALL_PROGRAM) 
robot.RunCodeCustom('rq_close_and_wait()', INSTRUCTION_CALL_PROGRAM)
robot.RunCodeCustom('unclamp()',INSTRUCTION_INSERT_CODE)
robot.MoveL(xyzrpw_2_pose([clamp_x,clamp_y,z_components_place[tail] + 50.0,0,180,0]))
robot.MoveJ(xyzrpw_2_pose([pallet_x[head],pallet_y[head], z_components_pick[head] + 150, 0, 180, 0]))
robot.setSpeed(50)
robot.MoveL(xyzrpw_2_pose([pallet_x[head]+3,pallet_y[head], z_components_pick[head] + 88, 0, 180, 0]))
robot.RunCodeCustom('rq_open_and_wait()', INSTRUCTION_CALL_PROGRAM)
robot.MoveJ(intermediate)
robot.MoveJ(home)

print('Done')
