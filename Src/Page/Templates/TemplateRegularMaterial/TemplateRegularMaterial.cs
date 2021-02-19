using Htmlilka;
using static Csml.YoutubeVideo;
using System.Collections.Generic;

namespace Csml {
    public sealed class TemplateRegularMaterial : TemplateRegularMaterial<TemplateRegularMaterial> {
        public TemplateRegularMaterial(IElement headerLogo, IEnumerable<IElement> leftSideElements, IEnumerable<IElement> rightSideElements) : base(headerLogo, leftSideElements, rightSideElements) { }
    }

    public class TemplateRegularMaterial<T> : TemplateLeftSideMenu<T> where T : TemplateRegularMaterial<T> {
        public TemplateRegularMaterial(IElement headerLogo, IEnumerable<IElement> leftSideElements, IEnumerable<IElement> rightSideElements) : base(headerLogo, leftSideElements, 260, rightSideElements, 60, 800, 64) { }

        private void CheckTitleImageAspect(IMaterial material) {
            var image = material.TitleImage;
            if(image != null) {
                var roi = image.GetRoi();
                if(roi != null && roi.Length > 0) {
                    if(!image.IsRoiFitsIntoWideRect(roi)) {
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
                .AddDiv(a => {
                    a.AddClasses("Header");
                    a.Add(new Title(material.Title, ". ").Generate(context));
                    if(material.TitleImage != null) {
                        //if(material.TitleImage is Player player) {
                        //    a.Add(player.GetImage().Generate(context));
                        //} else {
                            a.Add(material.TitleImage.Generate(context));
                        //}
                    }
                    a.Add(material.Description.Generate(context));
                })
                .Add(material.Content.Generate(context));
        }
    }
}