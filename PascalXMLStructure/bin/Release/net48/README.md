This library adds class to your solution that manages the
structure of Pascal VOC. Use as is. 

Customization has been done to include confidence and an evaluation property.
The evaluation property was used to signal if an AI solution has evaluated the object or not.(Can be disregarded)

Updated the solution with XML mangement. It can save and read XML PASCAL/VOC file format using the 
new methods.

Example:

Read XML: 

PascalXMLStructure annotationObject = PascalXMLStructure.ParseXML(filename);
Replace filename with the path to the file for example : "c:\temp\my_file.xml"

the result will be an annotation object with all the content of the file.

SaveXML:
PascalXMLStructure.SaveXML(filename, annotationObject.Annotations);

Read Yolo format
To read from yolo the image size, label information is needed.

> Syntax: PascalXMLStructure.ParseYolo("yololabelfile.txt",labels, <width>, <height>,<depth>);

width, height and depth as integer

the labels object is a string array in the order to down as the numbering in the yolo format. For example
0 : Cat
1: Dog
2: Phone
3: ...

The object will be the same as for Pascal. The missing information will be empty values.

Save to Yolo format:
The label information must be in a dictionary. it is used from the annotation object name of the class to
return to the number previously entered.

> Syntax: PascalXMLStructure.SaveYolo("labelfile.txt", pascalXMLStructure.Annotations, labelDict);

The result will be a XML/TXT file with the annotaionobject depending on the output format chosen.


------------------------
Release Notes:
------------------------
2024-02-05 - Changed class method names for ParseXML and SaveXML to ParsePascal and SavePascal. Added Yolo txt support for ParseYolo and SaveYolo. Conversion to Yolo or to Pascal format can be done by saving the annotation object. note data loss if saving to Yolo format.
2023-06-16 - Change the datastructure so that it is accessable and possible to read and write XML files. 
2023-06-16 - Updated namespace to PascalVOC
2023-06-16 - Added multiframework support.