using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Web;
using ZXing;
using System.IO;

namespace PayDemo.Models
{
    public class Util
    {
        public static string ToQRBase64(string txt)
        {
            var bmp = GetQRCodeByZXingNet(txt, 300, 300);

            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            bmp.Dispose();

            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// 生成二维码图片
        /// </summary>
        /// <param name="strMessage">要生成二维码的字符串</param>
        /// <param name="width">二维码图片宽度</param>
        /// <param name="height">二维码图片高度</param>
        /// <returns></returns>
        public static Bitmap GetQRCodeByZXingNet(String strMessage, Int32 width, Int32 height)
        {
            Bitmap result = null;
            try
            {
                BarcodeWriter barCodeWriter = new BarcodeWriter();
                barCodeWriter.Format = BarcodeFormat.QR_CODE;
                barCodeWriter.Options.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
                barCodeWriter.Options.Hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.H);
                barCodeWriter.Options.Height = height;
                barCodeWriter.Options.Width = width;
                barCodeWriter.Options.Margin = 0;
                ZXing.Common.BitMatrix bm = barCodeWriter.Encode(strMessage);
                result = barCodeWriter.Write(bm);
            }
            catch (Exception ex)
            {
                //异常输出
            }
            return result;
        }
    }
}