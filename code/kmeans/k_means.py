#Ryker Zierden
#CSci 1913 Project 1
#k means file
"""this module contains the algorithm for paletting the file as well as a few helper functions for it. It will create
'k' means and assign those means until the file stops changing """
from image_utils import *
from math import sqrt



def color_distance(color1,color2):
    """
    Gives the distance between two given colors
    :param color1: the first color in the comparison
    :param color2: the second color in the comparison
    :return: the value of the square root of the differences of each R,G,B value, which represents the 'distance'
    """
    return sqrt((color1[0] - color2[0])**2 + (color1[1] - color2[1])**2 + (color1[2]-color2[2])**2)


def generate_random_k_means_list(k):
    """
    Generates a random list of colors to use as the initial k means list
    :param k: the length of the list/how many colors are generated
    :return: a list of tuples, each representing a random color
    """
    random_k_means_list = []
    for i in range(k):
        random_k_means_list.append((random_color()))
    return random_k_means_list


def k_assignment(color,means_list):
    """
    Finds the color in the means list with the smallest 'distance' to a given color (using the color_distance function)
    :param color: the color to be compared to the means list
    :param means_list: the list of colors that represent each of the palettes (the list we're comparing to)
    :return: the index of the closest color in the means list to the given color
    """
    minimum_distance_index = 0
    for i in range(len(means_list)):
        if color_distance(color,means_list[i]) < color_distance(color, means_list[minimum_distance_index]):
            minimum_distance_index = i
    return minimum_distance_index


def average_color(list_of_colors):
    """
    Gives the average color of a list of colors by averaging the red, green, and blue values
    :param list_of_colors: the list of colors to be averaged
    :return: an integer value that is the sum of the red, green, and blue values in the list divided by the number of
    colors in the list (the average of each of the values)
    """
    number_of_colors = len(list_of_colors)
    sumr = 0
    sumg = 0
    sumb = 0
    for i in range(number_of_colors):
        sumr += list_of_colors[i][0]
        sumg += list_of_colors[i][1]
        sumb += list_of_colors[i][2]
    return (sumr//number_of_colors,sumg//number_of_colors,sumb//number_of_colors)


def k_means(image,k):
    """
    Conducts the k means algorithm, initializing the means_list and assignment_list and updating them repeatedly until
    the assignments list stops changing
    :param image: an image to run the algorithm on, provided in 2d list of list format ([x][y], x lists of y elements)
    :param k: number of colors to strip the image down to (number of values in the means list)
    :return: the final assignment_list list of lists and means_list for the given image and k value
    """
    means_list = generate_random_k_means_list(k)
    width,height = get_width_height(image)
    assignment_list = []
    for x in range(width):
        assignment_list.append([0]*height)
    for x in range(width):
        for y in range(height):
            assignment_list[x][y] = k_assignment(image[x][y],means_list)
    #initial assignments list, old assignment list, and means list is generated, now to the meat and potatoes of the algorithm
    assignments_equal = False
    while assignments_equal == False:
        assignments_equal = True
        means_list_old = means_list.copy()
        for i in range(k):
            kth_list_of_colors = []
            for x in range(width):
                for y in range(height):
                    if assignment_list[x][y] == i:
                        kth_list_of_colors.append((image[x][y]))
            if len(kth_list_of_colors) == 0:
                means_list[i] = random_color()
            else:
                means_list[i] = average_color(kth_list_of_colors)
        #just updated the means list, now back to the assignments list
        for x in range(width):
            for y in range(height):
                if assignments_equal == True and (k_assignment(image[x][y], means_list) != k_assignment(image[x][y], means_list_old)):
                    assignments_equal = False
                assignment_list[x][y] = k_assignment(image[x][y], means_list)
    #that should be it!! now to just return the final values!
    return means_list, assignment_list
