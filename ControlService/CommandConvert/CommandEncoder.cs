using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ControlService.CommandConvert
{
    public class CommandEncoder
    {
        public EncoderAligner Aligner;
        public EncoderRobot Robot;
        public EncoderOCR OCR;
        public EncoderLoadPort LoadPort;
        public Encoder_SmartTag SmartTag;
        public EncoderFFU FFU;
        public Encoder_CTU CTU;
        public Encoder_ELPT ELPT;
        public Encoder_ILPT ILPT;
        public Encoder_PTZ PTZ;
        public Encoder_FoupRobot FoupRobot;
        public Encoder_Shelf Shelf;
        public Encoder_WHR WHR;
        public Encoder_WTSAligner WTSAligner;
        public Encoder_E84 E84;



        private string Supplier;



        /// <summary>
        /// Encoder
        /// </summary>
        /// <param name="supplier"> Equipment supplier </param>
        public CommandEncoder(string supplier)
        {


            try
            {
                Supplier = supplier.ToUpper();

                Aligner = new EncoderAligner(Supplier);
                Robot = new EncoderRobot(Supplier);
                OCR = new EncoderOCR(Supplier);
                LoadPort = new EncoderLoadPort(Supplier, EncoderLoadPort.CommandMode.TDK_A);
                FFU = new EncoderFFU(Supplier);
                CTU = new Encoder_CTU(Supplier);
                ELPT = new Encoder_ELPT(Supplier);
                ILPT = new Encoder_ILPT(Supplier);
                PTZ = new Encoder_PTZ(Supplier);
                FoupRobot = new Encoder_FoupRobot(Supplier);
                Shelf = new Encoder_Shelf(Supplier);
                WHR = new Encoder_WHR(Supplier);
                WTSAligner = new Encoder_WTSAligner(Supplier);
                E84 = new Encoder_E84(Supplier);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {

            }
        }


    }
}
