function Force = getForce(crankAngle,crank,conrod,bore,pressure,mu)

% Calculate the force on the connecting rod of the piston cylinder

% as a function of the crank angle and pressure.

%

% Note: The zero crank angle reference position is the

% same as the piston top dead center (TDC) position, and

% the crank rotates in the clockwise direction.

% Author: RJZ

% Date: 2021.10.13
% define atmospheric pressure to account for relative pressure in piston
P_atm = 101300; %Pa

%% Calculate the Force

% convert crank angle to radians, store in theta
theta = pi*crankAngle/180;
% calculate beta, the third angle of the triangle made between height and
% the two links connecting the motor to the piston
beta = asin((crank/conrod).*sin(theta)); %radians
bore_area = pi * bore^2/4; %m^2
% find the pressure force, accounting for the fact that we need relative
% pressure
pressure_force = (pressure - P_atm).*bore_area; %N
% find the conrod_force obtained from force analysis in y direction
conrod_force = pressure_force./((cos(beta))+sign(sin(theta)).*mu.*sin(beta));

Force = conrod_force; % N
