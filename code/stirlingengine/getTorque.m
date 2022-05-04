function Torque = getTorque(crankAngle,crank,conrod,force)

% Calculate the torque on the connecting rod of the piston cylinder

% as a function of the crank angle and force.

%

% Note: The zero crank angle reference position is the

% same as the piston top dead center (TDC) position, and

% the crank rotates in the clockwise direction.

% Author: RJZ

% Date: 2021.10.13


%% Calculate the Torque

% convert crank angle to radians, store in theta
theta = pi*crankAngle/180;
% calculate beta, the third angle of the triangle made between height and
% the two links connecting the motor to the piston
beta = asin((crank/conrod).*sin(theta)); %radians
% calculate gamma, the angle between the conrod and crank
gamma = pi - theta - beta;
% T = F*r where F is the component of the conrod force that is normal to
% the crank
torque = force.*sin(gamma)*crank;

Torque = torque;
