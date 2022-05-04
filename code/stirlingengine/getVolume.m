function Vol = getVolume(crankAngle,crank,conrod,bore,clearanceHeight)

% Calculate the volume of the gas contained in a piston cylinder device

% as a function of the crank angle.

%

% Note: The zero crank angle reference position is the

% same as the piston top dead center (TDC) position, and

% the crank rotates in the clockwise direction.

% Author: RJZ

% Date: 2021.09.27

% 

% Key Variables:

% Provided in function parameters - crankAngle (theta), crank (length of 
% the crank), conrod (length of the connecting rod, bore (diameter of the 
% piston), clearanceHeight (distance between top of piston and top of 
% enclosure at TDC position)

% calculated intermediate values - y (distance between ground-crank
% connection and the conrod-piston connection)


 

%% Calculate the gas volume

% convert crank angle to radians, store in theta
theta = pi*crankAngle/180;
% calculate y
y = sqrt(crank^2 + conrod^2 - 2*crank*conrod*cos(pi-theta-asin((crank/conrod)*sin(theta))));
% calculate and return the volume of the cylinder
Vol = (pi*bore^2/4)*(clearanceHeight + crank + conrod - y);