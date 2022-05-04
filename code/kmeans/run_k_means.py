#Ryker Zierden
#CSci 1913 Project 1
#run k means file
"""This module runs the k means algorithm on an image and makes that image paletted to 'k' different colors. It is a
main function and therefore has no parameters or return values """
from k_means import *
from image_utils import *

file_name = input("enter the name of the file that you'd like to palette (must be a P3 ppm file (.ppm)):")
k = int(input("enter how many colors you'd like the final image to have:"))
output_name  = input("enter the name you'd like for the output file (must be a ppm filename (.ppm)):")
image = read_ppm(file_name)
means_list, assignment_list = k_means(image,k)
width,height = get_width_height(image)
for i in range(k):
    for x in range(width):
        for y in range(height):
            if assignment_list[x][y] == i:
                image[x][y] = means_list[i]
save_ppm(output_name,image)