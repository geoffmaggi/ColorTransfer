# ColorTransfer

  Based off Reinhard's Color Transfer Between Images
  http://www.cs.tau.ac.il/~turkel/imagepapers/ColorTransfer.pdf
 
Supports .jpg and .png images in RGB ColorSpace

To Do's:
 - Upload Images*
 - Convert Images From RGB to CIE LaB*
   - RGB -> XYZ //Skip
   - XYZ -> LMS //Skip
   - RGB -> LMS*
   - Eliminate LMS Skew (Skipped?)
   - LMS -> LaB*
 - Convert Images from CIE LaB to RGB*
   - LaB -> LMS*
   - LMS -> RGB*

 - Write Color Transfer Algorithm*
   - Mean of list for every color*
   - STD of list for every color*
   - Implement Algorithm*
   
 - Add LMS -> CIE CAM97*
 - Add CIE CAM97 -> LMS*
 
 - Spot check some examples*
 - Add basic security*

 ========== Bug fixes: ==================
   - NaN or out of range values in images*
=========================================