using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

/*=========================================
  
  Based off Reinhard's Color Transfer Between Images
  http://www.cs.tau.ac.il/~turkel/imagepapers/ColorTransfer.pdf
 
 * Supports .jpg and .png images in RGB ColorSpace

 * To Do's:
 *  - Upload Images*
 *  - Convert Images From RGB to CIE LaB*
 *    - RGB -> XYZ //Skip
 *    - XYZ -> LMS //Skip
 *    - RGB -> LMS*
 *    - Eliminate LMS Skew (Skipped?)
 *    - LMS -> LaB*
 *  - Convert Images from CIE LaB to RGB*
 *    - LaB -> LMS*
 *    - LMS -> RGB*
 * 
 *  - Write Color Transfer Algorithm*
 *    - Mean of list for every color*
 *    - STD of list for every color*
 *    - Implement Algorithm*
 *    
 *  - Add LMS -> CIE CAM97*
 *  - Add CIE CAM97 -> LMS*
 *  
 *  - Spot check some examples*
 *  - Add basic security*
 * 
 *  ========== Bug fixes: ==================
 *    - NaN or out of range values in images*
 * ========================================= */

namespace ColorTransfer {
  public partial class Default : System.Web.UI.Page {
    
    //Used to manipulate the JPEG bitmaps
    protected struct col {
      public float R;
      public float G;
      public float B;
    }
    
    protected void Page_Load(object sender, EventArgs e) {
			debug.Text = "";
    }

    protected void run(object sender, EventArgs e) {
      if (sourceFU.HasFile && targetFU.HasFile) {
        //source(RGB) -> sourceEdit(Lab)
        Bitmap source = new Bitmap(sourceFU.PostedFile.InputStream);
        Bitmap target = new Bitmap(targetFU.PostedFile.InputStream);

        // spaceDDL key:
        //  1: LAB
        //  2: CAM97
        //  3. LMS
        //  4. RGB

        //RGB
        col[,] sourceEdit = BitmapToCol(source);
        col[,] targetEdit = BitmapToCol(target);
        col[,] results = null;

        if (spaceDDL.SelectedValue == "1") { //LMS
          RGBtoLMS(ref sourceEdit);
          RGBtoLMS(ref targetEdit);

          LMStoLab(ref sourceEdit);
          LMStoLab(ref targetEdit);

          results = imgTrans(sourceEdit, targetEdit);

          LabtoLMS(ref results);
          LMStoRGB(ref results);
        }
        else if (spaceDDL.SelectedValue == "2") { //CAM97
          RGBtoLMS(ref sourceEdit);
          RGBtoLMS(ref targetEdit);

          LMStoCAM97(ref sourceEdit);
          LMStoCAM97(ref targetEdit);

          results = imgTrans(sourceEdit, targetEdit);

          CAM97toLMS(ref results);
          LMStoRGB(ref results);
        }
        else if (spaceDDL.SelectedValue == "3") { //LMS
          RGBtoLMS(ref sourceEdit);
          RGBtoLMS(ref targetEdit);

          results = imgTrans(sourceEdit, targetEdit);

          LMStoRGB(ref results);
        }
        else { //RGB
          results = imgTrans(sourceEdit, targetEdit);
        }

        Bitmap result = colToBitmap(results);
				
				//Show the images
        showImage(source);
				showImage(target);
				showImage(result);
      }
			else {
				debug.Text = "Please select an source and target file";
			}
    }

    protected col[,] imgTrans(col[,] sourceEdit, col[,] targetEdit) {
      col[,] results = sourceEdit; //Deep copy
      col sourceMean = calcMean(ref sourceEdit);
      col targetMean = calcMean(ref targetEdit);
      col sourceSD = calcStdDiv(ref sourceEdit);
      col targetSD = calcStdDiv(ref targetEdit);


      //First subtract mean from points
      for (int i = 0; i < results.GetLength(0); i++) {
        for (int j = 0; j < results.GetLength(1); j++) {
          results[i, j].R -= sourceMean.R;
          results[i, j].G -= sourceMean.G;
          results[i, j].B -= sourceMean.B;
        }
      }

      //scale the data points comprising the synthetic
      //image by factors determined by the respective standard deviations
      for (int i = 0; i < results.GetLength(0); i++) {
        for (int j = 0; j < results.GetLength(1); j++) {
          results[i, j].R *= (targetSD.R / sourceSD.R);
          results[i, j].G *= (targetSD.G / sourceSD.G);
          results[i, j].B *= (targetSD.B / sourceSD.B);
        }
      }

      //add the averages computed for the photograph
      for (int i = 0; i < results.GetLength(0); i++) {
        for (int j = 0; j < results.GetLength(1); j++) {
          results[i, j].R += targetMean.R;
          results[i, j].G += targetMean.G;
          results[i, j].B += targetMean.B;
        }
      }

      return results;
    }

    protected col[,] BitmapToCol(Bitmap img) {
      col[,] ret = new col[img.Width, img.Height];
      for (int i = 0; i < img.Width; i++) {
        for (int j = 0; j < img.Height; j++) {
          Color pixel = img.GetPixel(i, j);
          (ret[i, j]).R = Convert.ToInt16(pixel.R);
          (ret[i, j]).G = Convert.ToInt16(pixel.G);
          (ret[i, j]).B = Convert.ToInt16(pixel.B);
        }
      }
      return ret;
    }

    protected Bitmap colToBitmap(col[,] img) {
      Bitmap ret = new Bitmap(img.GetLength(0), img.GetLength(1));
      for (int i = 0; i < img.GetLength(0); i++) {
        for (int j = 0; j < img.GetLength(1); j++) {
          img[i, j].R = img[i, j].R != img[i, j].R ? 0 : img[i, j].R > 255 ? 255 : img[i, j].R < 0 ? 0 : img[i, j].R;
          img[i, j].G = img[i, j].G != img[i, j].G ? 0 : img[i, j].G > 255 ? 255 : img[i, j].G < 0 ? 0 : img[i, j].G;
          img[i, j].B = img[i, j].B != img[i, j].B ? 0 : img[i, j].B > 255 ? 255 : img[i, j].B < 0 ? 0 : img[i, j].B;
          
          int red = Convert.ToInt32(img[i, j].R);
          int green = Convert.ToInt32(img[i, j].G);
          int blue = Convert.ToInt32(img[i, j].B);
          
          Color px = Color.FromArgb(red, green, blue);
          ret.SetPixel(i, j, px);
        }
      }
      return ret;
    }
    
    protected void RGBtoLMS(ref col[,] img) {
      for (int i = 0; i < img.GetLength(0); i++) {
        for (int j = 0; j < img.GetLength(1); j++) {
          float[] RGB = new float[3] { (img[i,j]).R, (img[i,j]).G, (img[i,j]).B };
          float[,] toLMS = new float [3,3]
          { {(float)0.3811, (float)0.5783, (float)0.0402},
            {(float)0.1967, (float)0.7244, (float)0.0782},
            {(float)0.0241, (float)0.1288, (float)0.8444}
          };

          float[] LMS = matMult(RGB, toLMS);

          img[i, j].R = LMS[0];
          img[i, j].G = LMS[1];
          img[i, j].B = LMS[2];
        }
      }
    }

    protected void LMStoCAM97(ref col[,] img) {
      for (int i = 0; i < img.GetLength(0); i++) {
        for (int j = 0; j < img.GetLength(1); j++) {
          float[] LMS = new float[3] { (img[i, j]).R, (img[i, j]).G, (img[i, j]).B };
          float[,] toCAM97 = new float[3, 3]
          { {(float)2.000, (float)1.000, (float)0.0500},
            {(float)1.000, (float)-1.0900, (float)0.0900},
            {(float)0.1100, (float)0.1100, (float)-0.2200}
          };

          float[] CAM97 = matMult(LMS, toCAM97);

          img[i, j].R = CAM97[0];
          img[i, j].G = CAM97[1];
          img[i, j].B = CAM97[2];
        }
      }
    }

    protected void LMStoLab(ref col[,] img) {
      for (int i = 0; i < img.GetLength(0); i++) {
        for (int j = 0; j < img.GetLength(1); j++) {
          float[] LMS = new float[3] { (float)Math.Log10((img[i, j]).R), (float)Math.Log10((img[i, j]).G), (float)Math.Log10((img[i, j]).B) };
          float[,] toLab1 = new float[3, 3]
          { {(float)1, (float)1, (float)1},
            {(float)1, (float)1, (float)-2},
            {(float)1, (float)-1, (float)0.0}
          };
          float[,] toLab2 = new float[3, 3]
          { {(float)(1/Math.Sqrt(3)), (float)0.0, (float)0.0},
            {(float)0.0, (float)(1/Math.Sqrt(6)), (float)0.0},
            {(float)0.0, (float)0.0, (float)(1/Math.Sqrt(2))}
          };

          float[] Lab = matMult(LMS, toLab1);
          Lab = matMult(Lab, toLab2);

          img[i, j].R = Lab[0];
          img[i, j].G = Lab[1];
          img[i, j].B = Lab[2];
        }
      }
    }

    protected void LabtoLMS(ref col[,] img) {
      for (int i = 0; i < img.GetLength(0); i++) {
        for (int j = 0; j < img.GetLength(1); j++) {
          float[] Lab = new float[3] { (img[i, j]).R, (img[i, j]).G, (img[i, j]).B };
          float[,] toLMS1 = new float[3, 3]
          { {(float)(Math.Sqrt(3)/3), (float)0.0, (float)0.0},
            {(float)0.0, (float)(Math.Sqrt(6)/6), (float)0.0},
            {(float)0.0, (float)0.0, (float)(Math.Sqrt(2)/2)}
          };
          float[,] toLMS2 = new float[3, 3]
          { {(float)1, (float)1, (float)1},
            {(float)1, (float)1, (float)-1},
            {(float)1, (float)-2, (float)0.0}
          };

          float[] LMS = matMult(Lab, toLMS1);
          LMS = matMult(LMS, toLMS2);

          img[i, j].R = (float)Math.Pow(10, LMS[0]);
          img[i, j].G = (float)Math.Pow(10, LMS[1]);
          img[i, j].B = (float)Math.Pow(10, LMS[2]);
        }
      }
    }

    protected void CAM97toLMS(ref col[,] img) {
      for (int i = 0; i < img.GetLength(0); i++) {
        for (int j = 0; j < img.GetLength(1); j++) {
          float[] CAM97 = new float[3] { img[i, j].R, img[i, j].G, img[i, j].B };
          //Inverse was not provided so I had to calculate it
          float[,] toLMS = new float[3, 3]
          { {(float)0.3279, (float)0.3216, (float)0.2061},
            {(float)0.3279, (float)-0.6353, (float)-0.1854},
            {(float)0.3279, (float)-0.1569, (float)-4.5351}
          };

          float[] LMS = matMult(CAM97, toLMS);
          img[i, j].R = LMS[0];
          img[i, j].G = LMS[1];
          img[i, j].B = LMS[2];
        }
      }
    }

    protected void LMStoRGB(ref col[,] img) {
      for (int i = 0; i < img.GetLength(0); i++) {
        for (int j = 0; j < img.GetLength(1); j++) {
          float[] LMS = new float[3] { img[i, j].R, img[i, j].G, img[i, j].B };
          float[,] toRGB = new float[3, 3]
          { {(float)4.4679, (float)-3.5873, (float)0.1193},
            {(float)-1.2186, (float)2.3809, (float)-0.1624},
            {(float)0.0497, (float)-0.2439, (float)1.2045}
          };

          float[] RGB = matMult(LMS, toRGB);
          img[i, j].R = RGB[0];
          img[i, j].G = RGB[1];
          img[i, j].B = RGB[2];
        }
      }
    }

    protected col calcMean(ref col[,] img) {
      col ret = new col();
      ret.R = 0;
      ret.G = 0;
      ret.B = 0;

      for (int i = 0; i < img.GetLength(0); i++) {
        for (int j = 0; j < img.GetLength(1); j++) {
          if(img[i,j].R == img[i,j].R)
            ret.R += img[i, j].R;
          if (img[i, j].G == img[i, j].G)
            ret.G += img[i, j].G;
          if (img[i, j].B == img[i, j].B)
            ret.B += img[i, j].B;
        }
      }

      ret.R /= (img.GetLength(0) * img.GetLength(1));
      ret.G /= (img.GetLength(0) * img.GetLength(1));
      ret.B /= (img.GetLength(0) * img.GetLength(1));

      return ret;
    }

    protected col calcStdDiv(ref col[,] img) {
      col ret = new col();
      ret.R = 0;
      ret.G = 0;
      ret.B = 0;

      //1. Calc Mean
      col mean = calcMean(ref img);

      //2. subtract the Mean and square the result.
      //3. Then work out the mean of those squared differences.
      for (int i = 0; i < img.GetLength(0); i++) {
        for (int j = 0; j < img.GetLength(1); j++) {
          if (img[i, j].R == img[i, j].R)
            ret.R += (img[i, j].R - mean.R) * (img[i, j].R - mean.R);
          if (img[i, j].G == img[i, j].G)
            ret.G += (img[i, j].G - mean.G) * (img[i, j].G - mean.G);
          if (img[i, j].B == img[i, j].B)
            ret.B += (img[i, j].B - mean.B) * (img[i, j].B - mean.B);
        }
      }

      ret.R /= (img.GetLength(0) * img.GetLength(1));
      ret.G /= (img.GetLength(0) * img.GetLength(1));
      ret.B /= (img.GetLength(0) * img.GetLength(1));

      //4. Take the square root of that and we are done!
      ret.R = (float)Math.Sqrt(ret.R);
      ret.G = (float)Math.Sqrt(ret.G);
      ret.B = (float)Math.Sqrt(ret.B);


      return ret;
    }

    protected float[] matMult(float[] a, float[,] b) {
      float[] ret = new float[3] {0,0,0};

      for (int i = 0; i <= 2; i++) {
        for (int j = 0; j <= 2; j++) {
          ret[i] += a[j] * b[i,j];
        }
      }
      return ret;
    }

		//Addes the image to ImageUP
		protected void showImage(Bitmap img) {
			MemoryStream ms = new MemoryStream();
			img.Save(ms, ImageFormat.Jpeg);
			var base64Data = Convert.ToBase64String(ms.ToArray());

			System.Web.UI.WebControls.Image newImg = new System.Web.UI.WebControls.Image();
			newImg.ImageUrl = "data:image/jpg;base64," + base64Data;
			newImg.Visible = true;
			imageUP.ContentTemplateContainer.Controls.Add(newImg);
		}

		protected void clear(object sender, EventArgs e) {
			Response.Redirect(Request.RawUrl); //Just refresh
		}

  }
}