# .NET CAM Validation Library

Content Assembly Mechanism (CAM) is an XML-based standard for validation. Much like XSD, but much more powerful. CAM is a product of the OASIS Content Assembly Technical Committee.

For in-depth information on CAM, please refer to Wikipedia and/or Oasis, links below:

- http://en.wikipedia.org/wiki/Content_Assembly_Mechanism
- https://www.oasis-open.org/committees/tc_home.php?wg_abbrev=cam

As there is only a Java libarary in existance, there was a desire to write a library for .NET.  This is a very basic implemenatation of the CAM specification.  Much of the specification is missing.  Currenty this library supports some functionality of the following predicates:

- makeOptional()
- makeMandatory()
- makeNillable()
- setLength()
- setNumberRange()
- restrictValues()
- setNumberMask()
- setDateMask()

Please feel free to extend this library as required.

Example XML and CAM has been updated from here: 

- http://www.ibm.com/developerworks/library/x-camval/

![Alt text](/screenshot01.png?raw=true "Screen Shot")
