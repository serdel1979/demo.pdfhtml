using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;




using HtmlAgilityPack;
using Aspose.Pdf;
using ConvertPDF.Models;


namespace ConvertPDF.Controllers
{
    public class PdfController : Controller
    {
        // GET: PdfController
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                string htmlContent = ConvertPdfToHtml(path);
                var model = new PdfViewModel { HtmlContent = htmlContent };

                return View("Display", model);
            }

            return RedirectToAction("Index");
        }



        private string ConvertPdfToHtml(string pdfPath)
        {
            var doc = new HtmlDocument();
            var body = doc.DocumentNode.AppendChild(doc.CreateElement("body"));

            Document pdfDocument = new Document(pdfPath);
            int imageCounter = 1;

            foreach (var page in pdfDocument.Pages)
            {
                // Extraer texto
                var absorber = new Aspose.Pdf.Text.TextAbsorber();
                page.Accept(absorber);
                string text = absorber.Text;
                var textNode = doc.CreateElement("p");
                textNode.InnerHtml = text;
                body.AppendChild(textNode);

                // Extraer imágenes
                foreach (XImage image in page.Resources.Images)
                {
                    using (MemoryStream imageStream = new MemoryStream())
                    {
                        image.Save(imageStream, System.Drawing.Imaging.ImageFormat.Png);
                        string imagePath = SaveImage(imageStream, page.Number, imageCounter++);
                        var imgNode = doc.CreateElement("img");
                        imgNode.SetAttributeValue("src", Url.Content(imagePath));
                        body.AppendChild(imgNode);
                    }
                }

                // Reiniciar el contador de imágenes para la siguiente página
                imageCounter = 1;
            }

            using (StringWriter writer = new StringWriter())
            {
                doc.Save(writer);
                return writer.ToString();
            }
        }

        private string SaveImage(MemoryStream imageStream, int pageNumber, int imageCounter)
        {
            string imagePath = Path.Combine("wwwroot/images", $"Page{pageNumber}_Image{imageCounter}.png");
            string relativePath = $"/images/Page{pageNumber}_Image{imageCounter}.png";

            using (var fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write))
            {
                imageStream.WriteTo(fileStream);
            }

            return relativePath;
        }













    }
}
