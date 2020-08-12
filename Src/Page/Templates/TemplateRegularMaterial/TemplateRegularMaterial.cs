using Htmlilka;

namespace Csml {
    public sealed class TemplateRegularMaterial : TemplateRegularMaterial<TemplateRegularMaterial> {
        public TemplateRegularMaterial(IElement leftSideMenu) : base(leftSideMenu) { }
    }

    public class TemplateRegularMaterial<T> : TemplateLeftSideMenu<T> where T: TemplateRegularMaterial<T> {
        public TemplateRegularMaterial(IElement leftSideMenu) : base(leftSideMenu,800,64) { }

        private void CheckTitleImageAspect(IMaterial material) {
            var image = material.TitleImage;
            if (image != null) {
                var roi = image.GetRoi();
                if (roi != null && roi.Length > 0) {
                    if (!image.IsRoiFitsIntoWideRect(roi)) {
                        Log.Warning.OnObject(material, $"Invalid ROI for material TitleImage. Material title = {material.Title}");
                    }
                } else {
                    Log.Warning.OnObject(material, $"ROI required for material TitleImage. Material title = {material.Title}");
                }
            }
        }

        public override Tag WriteMaterial(Context context, IMaterial material) {
            CheckTitleImageAspect(material);

            return new Tag("div")
                .AddDiv(a=> {
                    a.AddClasses("Header");
                    a.AddTag("h1", b => {
                        b.AddClasses("Title");
                        b.AddPureHtmlNode(material.Title.Replace(".", "<wbr/>."));
                    });
                    if (material.TitleImage != null) {
                        a.Add(material.TitleImage.Generate(context));
                    }
                    a.Add(material.Description.Generate(context));
                })
                .Add(material.Content.Generate(context));
        }
    }
}