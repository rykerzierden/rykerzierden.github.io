function Roots = getRoots(piston_crank_angle,torque, average_torque)

% Author: RJZ

% Date: 2021.09.27

% the following for loop finds the exact roots of the function. This is
% to avoid the sometimes quite tedious syntax needed to get fzero to work
% properly (even though it's more lines of code)
minTheta1 = 0;
minTheta2 = 0;
torqueSize = size(torque,2);
for i = 1:torqueSize
    % set lastTorque variable for comparison as well as initialize our
    % minimum difference values (only on first loop through)
    if i < 2
       lastTorque = torque(i);
       minDiff1 = abs(average_torque - torque(i));
       minDiff2 = minDiff1;
    else
       lastTorque = torque(i - 1);
    end
    % calculate the current distance between the torque and the average
    % torque
    currDiff = abs(torque(i) - average_torque);
    % compare the current distance with the minimum difference with
    % separate cases for when the curve is climbing vs falling (to obtain
    % both roots). If this value is the current minimum, write the theta
    % value to the minTheta variable and update the minDiff variable for
    % this case
    if torque(i) - lastTorque > 0
        if (currDiff < minDiff1)
            minDiff1 = currDiff;
            minTheta1 = piston_crank_angle(i);
        end
    else
        if currDiff < minDiff2
            minDiff2 = currDiff;
            minTheta2 = piston_crank_angle(i);
        end
    end
end

Roots = [minTheta1 minTheta2];



 

