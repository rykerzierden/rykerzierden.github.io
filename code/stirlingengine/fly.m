% Flywheel Calculation Script
% ME4053 Project 1
% Author: Ryker Zierden
% Date: 10 12 2021

%% Initialization
%Initialize this run
clear
clc
 
%initialize crank angles, NOTE: 0 degrees is TDC
piston_crank_angle = 0:0.01:360; 
displacer_crank_angle = 90:0.01:450;
%% Design parameters

% Piston
piston_crankshaft = 0.050; %m
piston_conrod = 0.150; %m
piston_clearance = 0.1; %m

% Displacer
disp_crankshaft = 0.050; %m
disp_conrod = 0.24; %m
disp_clearance = 0.01; %m

% Other parameters
bore_diameter = 0.10; %m
phase_shift = pi/2; %radians
T_high = 850; %K
T_low = 400; %K
P_min = 101300; %Pa, BDC
regen_dead_volume = 0.0001; %m^3
flywheel_width = 0.025; %m
flywheel_rim_thickness = 0.050; %m
Cf = 0.002; %AU
mu = 0.10; %AU
angular_velocity = 800; %RPM

%% Get Volumes
% use getVolume function to get the volume across all angles for piston and
% displacer
piston_volume = getVolume(piston_crank_angle,piston_crankshaft,piston_conrod,bore_diameter,piston_clearance);
displacer_volume = getVolume(displacer_crank_angle,disp_crankshaft,disp_conrod,bore_diameter,disp_clearance);
% the displacer volume represents the volume if the piston was not there, we
% want the volume between the piston and displacer -> subtract the two
compression_volume = piston_volume - displacer_volume; %m^3
% expansion volume is just displacer volume, but it will work more intuitively
% if the variable is named differently
expansion_volume = displacer_volume; %m^3
% generate volume plot
hold on
plot(piston_crank_angle,piston_volume,"m-")
plot(piston_crank_angle,compression_volume,"r-")
plot(piston_crank_angle,expansion_volume,"b-")
legend("Total Volume","Compression Volume", "Expansion Volume")
grid on
xlabel("Crank Angle (degrees)")
ylabel("Volume (m^3)")
xlim([0 360])
title("Stirling Engine Volume")
set(gca,'xminortick','on')
set(gca,'yminortick','on')
hold off
%% Get Pressure
% to get the pressure, we first must find the mass -> ideal gas law
% assumption analysis at BDC position (assume mass is constant 
% throughout cycle)PV = mRT -> m = PV/RT, assume pressure is constant 
% across all three areas. mtotal = (P/R)(Vc/Tc + Ve/Te + Vr/Tr)
R_air = 287; %J/kgK
% find T_regen, needed for mass calculation
T_regen = (T_high + T_low)/2;
% find m_total, 100 data points taken per degree of rotation -> index =
% theta * 100. BDC is 180 for piston, comp volume in terms of piston angle
% when indexing
m_total = (P_min/R_air) * (compression_volume(round(size(compression_volume,2)/2))/T_low + expansion_volume(round(size(expansion_volume,2)/2))/T_high +regen_dead_volume/T_regen); %kg
% assuming mass, temperature is constant, P = mRT/V = mR(Tc/Vc  + Te/Ve +
% Vr/Tr)
regen_dead_volume_matrix = (regen_dead_volume.*ones(1,size(expansion_volume,2)));
pressure = R_air * m_total./(compression_volume./T_low + expansion_volume./T_high + (regen_dead_volume_matrix)./T_regen);
% generate pressure plot
figure()
hold on
plot(piston_crank_angle,pressure,"g-")
legend("Pressure")
grid on
xlabel("Crank Angle (degrees)")
ylabel("Pressure (Pa)")
xlim([0 360])
title("Stirling Engine Pressure")
legend("Pressure")
set(gca,'xminortick','on')
set(gca,'yminortick','on')
hold off

%% Get Force
% run the force function on the crank angle and pressure to get
% the force on the conrod throughout the entire motion of the mechanism
force = getForce(piston_crank_angle,piston_crankshaft,piston_conrod,bore_diameter,pressure,mu);
% generate force plot
figure()
hold on
plot(piston_crank_angle,force,"m-")
legend("Force")
grid on
xlabel("Crank Angle (degrees)")
ylabel("Force (N)")
xlim([0 360])
title("Stirling Engine Force")
legend("Force")
set(gca,'xminortick','on')
set(gca,'yminortick','on')
hold off

%% Get Torque
% run the torque function on the crank angle and force to get the torque on
% the motor throughout the motion of the mechanism

torque = getTorque(piston_crank_angle,piston_crankshaft,piston_conrod,force);%Nm



%% Get Change in Kinetic Energy (Above Average)
% the area under the torque/crank angle curve represents energy provided by
% the crank shaft. By finding the average, we can see where the energy is that we
% need to restore to the system with the inertia of the flywheel


% calculate average torque using mvt

average_torque = (1/360) * trapz(piston_crank_angle,torque) %Nm


% now get the roots using the getRoots function, which is a custom function
% that avoids the need to use fzero and thus circumvents the need to
% get the function feature to work in MATLAB (which is quite tedious most
% of the time)
roots = getRoots(piston_crank_angle, torque, average_torque); %Nm
root1 = roots(1);
root2 = roots(2);

% now that we have the roots, we can use the mean value theorem to once
% again to find the area over the average torque, which is the energy that
% must be supplied by the flywheel

% get subset of crank angles and torque (NOTE: there are steps here to make
% the step changeable at the top of the function for more or less precision
% that make this messier than it has to be. The result is the ability to
% change the precision/number of angles at will/even use negative steps if
% desired in a later use case)
angle_step = abs(piston_crank_angle(2) - piston_crank_angle(1));
root_crank_angles = deg2rad(root1):deg2rad(angle_step):deg2rad(root2);
root_torque = torque((root1)*(1/angle_step)+1:(root2)*(1/angle_step)+1);

% generate plot of torque with average torque and roots shown
figure()
hold on
plot (piston_crank_angle,torque,"b-")
yline(average_torque,"r-")
plot ([root1 root2], average_torque,"ro")
grid on
xlabel("Crank Angle (degrees)")
ylabel("Torque (Nm)")
xlim([0 360])
title("Stirling Engine Torque")
legend("Torque","Average Torque")
set(gca,'xminortick','on')
set(gca,'yminortick','on')
hold off
% plot(root_crank_angles, (root_torque))
% use mean value theorum to find delta KE
delta_KE = trapz(root_crank_angles,root_torque-average_torque); % J

%% Find J from KE/theta
% J = KE/(w^2 * Cf)
% need to convert average angular velocity to radians
omega_average = angular_velocity * 1/60 * 2 * pi; % rad/s

% solve for J
J = delta_KE/(omega_average^2 * Cf) % kg * m^2

%% Find Do, Di, m_flywheel from J
% the flywheel will be made of steel, so we'll need to get the density of
% steel for this calculation
steel_density = 8050;  % kg/m^3

% Do has a fairly complicated equation -> solve with fzero
% combine constants
c = (steel_density * flywheel_width/32)*pi;
% define a function to find the zeros of
Do_func = @(Do)(c * (Do^4 - (Do - 2*flywheel_rim_thickness)^4)-J);
% call fzero to find Do
Do = fzero(Do_func, 0)
Di = Do - 2*flywheel_rim_thickness
% calculate m_flywheel using volume * density
% m = volume*density
m_flywheel = (Do^4 - (Do - 2*flywheel_rim_thickness)^4)* (flywheel_width/4)*pi*steel_density

%% Find Average Power
% P = integral under pdV curve/time -> use trapz with piston_volume and pressure
% to find work then divide by time for one rotation

period = 1/800 * 60; % seconds/cycle
work = trapz(piston_volume,pressure); %J
average_power = work/period %W


%% Plot PdV
figure()
hold on
 plot(piston_volume,pressure) % plot the actual curve (Schmidt analysis)
% find minimum and maximum volumes from piston_volume array
max_volume = max(piston_volume);
min_volume = min(piston_volume);

% use ideal gas law to get max pressure from minimum volume
max_pressure = m_total*R_air*T_high/min_volume;
min_pressure = m_total*R_air*T_low/max_volume;
% set ideal_volume_range based on min and max values
ideal_volume = round(min_volume,6):0.000001:round(max_volume,6);
% calculate the isothermal pressure for expansion and compression using
% isothermal function
ideal_pressure_expansion = isothermal(min_pressure,max_volume,ideal_volume);
ideal_pressure_compression = isothermal(max_pressure,min_volume,ideal_volume);
% plot the ideal stirling cycle, set plot values
plot(ideal_volume,ideal_pressure_expansion,"m-","DisplayName","isothermal expansion")
plot(ideal_volume,ideal_pressure_compression,"m-","DisplayName","isothermal compression")

plot(min_volume * ones(2), [max(ideal_pressure_expansion) max_pressure],"m-","DisplayName","isochoric cooling")
plot(max_volume * ones(2), [min_pressure min(ideal_pressure_compression)],"m-","DisplayName", "isochoric heating")

legend("Force")
grid on
xlabel("Volume (m^3)")
ylabel("Pressure (Pa)")
title("Schmidt Analysis vs Ideal Stirling Cycle")
legend("Schmidt Analysis","Ideal Stirling Cycle")
set(gca,'xminortick','on')
set(gca,'yminortick','on')
hold off
% finally, calculate the ideal work of the cycle using trapz

ideal_work = trapz(ideal_volume,ideal_pressure_compression) - trapz(ideal_volume,ideal_pressure_expansion);
Stirling_Cycle_Efficiency = (work/ideal_work) * 100; % (%)

