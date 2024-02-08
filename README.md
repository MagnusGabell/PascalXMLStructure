# PascalXMLStructure
This package was originally for PascalVOC format but also now include Yolo format.

To use this add the solution to your project.
The solution contains a PascalVOC class and four support methods.

ParsePascal
SavePascal
ParseYolo
SaveYolo

It is possible to read a Yolo format and use the Save Pascal to save it to Pascal format.
It is also possible to take a Pascal format and save as Yolo.

Note: When saving to Yolo format from Pascal/VOC you loose data since the Yolo format contains less information.
To save as Yolo one must provide, output filename, imageWidth, imageHeight, class_lable_file

The output format is as follows:
class centerX centerY boxWidth boxHeight confidence

center values are the center of a box and normalized to the image total width and height. Same for the BoxHeight and BoxWidth.
The data is separated by space. The decimal char is period <.>

Package is available at nuget too.
