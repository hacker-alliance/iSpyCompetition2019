using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accusoft.FormFixSdk;
using Accusoft.FormDirectorSdk;
using Accusoft.ImagXpressSdk;

namespace SpyDetector
{
    class Process
    {
        IdentificationProcessor idProcessor;
        FormFix formFix = new FormFix();
        ImageX image;

        public void FFLicensing(FormFix formFix)
        {
            formFix.Licensing.SetOEMLicenseKey("2.0.EGLNkOkbPedNtNPbjNJNtx5etmaElIkXrxAmdGPKVGaNTm5OjyrXlQl65XANLOrIdejQ5KrQTSAXUXU65OVxVKJmJGA6JEU2PNlXd6dGlIgeTIg6UmVeaGaXPeVm5xUQrILxJblbUKPKPblKlyPSVGgQlGjKJIrXgNV2gSLOjEUSPEtxtEdxTOPNU2kyPXabU25GrbtSPxtSlKgxtNlK5bj2keaQTOUOVGaePeAQJKkErKlGT6geLOgSdmkEJETXdOPXkXl2VKLGL6rIkxAS5mtNaE5xUGPEtbTGVO5xgxA2JQaGtxTSVbrGA25yPxJGlOAS5xLIgOjegePyJbAX5yPSASUeJ6tXkmAedGLGTNjQUSgQkKUbdGLxLIAQT2jGrKtykSrbk2LxlIdXdNJek6TQVbPXVbaeaxrmLOPQayAKPNJx5bgXUGLQjKg6kekKJ6kQPyayUOU65et2VSPOLy5EUbdePEkxAyLGtSrykK5e5KUOANrejSVyAXdGLxd65ytOLbaGkNaeJmlKUmLXUeTNPOVSkKtSAml6JQkxrXrmVNTKjKTxJbLElxkm56rxjSkIPkS");
        }

        public void IXLicensing(ImagXpress iXpress)
        {
            iXpress.Licensing.SetOEMLicenseKey("2.0.EIDtFBFBXbxMSIx8ptcdztk5krHdsbYRFIHMFA7MDMzMYAFrXBHMqrC4xiXdZikhXiYwHr7ADBDrx8DbD5sACdFRz8ZwSIZ4C4qdZ3XiHizdpwD5sdFrxbDbF5DdHrzBS3CRYhZIChXd7MHRHiz3FAzd7rCdz8Dd7wXbZdFMCdDbcMYdz5CtYhp8C4s3C3Y3s4qRFAFhZbcAxBx5k8SiX8xMc5xRzdqbCBF3DtZBp4qdXrqIzAq87iSRcMCd7hZ8HbYhqhkhZ3cMX3HIZRzix4zh7hqMp3Hhp8sIsIsdxIs3XicrzrXAFrXBXICiYbpRsMs4cBzMFrx4F8qMFrqIs8DrCdq4xAHhkhF8sMsdC8cAkrS4pAz5kdxd7hzbHBStcIYtDrZ3HrxAH8zhZiq3Y3FIY47MFbHMSBk3ZApiDAXik8z8X4Y5pr7IkRYbHbY8qtF5HMFRSdH3sRFBH8YMqISMCRz5CdCIsBS3H4CBStSdSrsiF8S8qIzhDipAxBciSd75FhHBSbHAxbq874kMYtH5YBSdx3pRxbxbxdpA73x8qdcwqrCMZBstHdD8p8ZrHd75q4k8YOkMR");
        }
        public void FDLicensing(FormDirector formDirector)
        {
            formDirector.Licensing.SetOEMLicenseKey("2.0.E6dKRyMXM7REMwRui0kQsCRuGjd94EsEUQ2ui04ucKMaJyc0duPas7d9UCF75wMY20UEsjG756Ua4CsXRaGKUa2KHSk9UXdYcQ4uP9iwi94QiaMj5C2aPjJjPu5K4jJK5jPQHacQUaRK4YdC4KRCd9MKUKJKRXUEM9GSiQJwFyRQc6c92YRYMXJKGYdEcXs92EPQsKH7JwkXUycE50ij2a2acj2CGy46M0GKiuGCJaHK5X204jk659GaJQdj2KkYH95K2ai9GuJQPwF6HykwG6c6cYJXkKWwcEcjGQduRasaGyPaGYdwkC4Cdj5XkSH6H05w27G7k7RQ2XPCRQRwUSkQPQk6kC57UaRjU6kwdC5KPwR9ijsyFYPYR7caMYHKiC4QJa5KR0dyPjFYP0F6sXUac0Ga5XiEdCiaH0REUakE4CHXU6P6dX2ycX5as6HXiSMERwc6swkXdKkSF0JCHaUaPCUCHKHYi0MKGYiC20Jwi7Uy2Yc6H9GEUyMuP0Mj5XPa4aRXR72SkQ57cycQMC57UjF7s0kScwMYkwi9JK5a2K4KFXP0i9cCs74YGdu");
        }

        public void IdentificationSettings()
        {
            FFLicensing(formFix);

            idProcessor.ReadChecksum += new ReadChecksumEventHandler(ReadChecksum);
        }

        public string ProcessTheForm(ImagXpress iXpress, FormSetFile formSetFile, string imageFileToUse)
        {
            foreach (FormDefinition formDefinition in formSetFile.FormDefinitions)
            {
                
            }
        }

        void formModel_ReadFormImage(object sender, ReadFormImageEventArgs e)
        {
            FormModel formMode = (FormModel)sender;
            FormDefinitionFile fd = (FormDefinitionFile)formMode.UserTag;
            e.FormImage = FormImage.FromHdib(fd.TemplateImages[0].Hdib, true, formFix);
        }

        void ReadChecksum(object sender, ReadChecksumEventArgs e)
        {
            e.Checksum = 0;
        }
    }
}
