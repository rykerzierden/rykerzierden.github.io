% Calculates the pressure for an isobaric process given the volume and intial
% conditions
%
% Date: 10/6/2021
% Author: Ryker Zierden (in collaboration with in-class group 5)


function pressure = isothermal(pressure_initial, volume_initial, volume)

hold = pressure_initial*volume_initial;

pressure = hold ./ volume;

end