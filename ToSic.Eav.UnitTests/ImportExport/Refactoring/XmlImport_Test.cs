using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Implementations.UserInformation;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.ImportExport.Refactoring;
using ToSic.Eav.ImportExport.Refactoring.Options;

namespace ToSic.Eav.UnitTests.ImportExport.Refactoring
{
    [TestClass]
    public class XmlImport_Test
    {
        #region Settings

        private readonly int ZoneId = 1;
        private readonly int AppId = 1;
        private readonly int ContentTypeSimpleContent = 13;
        private Guid SingleItemGuid = new Guid("31d93b03-cfb3-483b-8134-e08bbee9cd2c");
        private string UserNameOfTestingScript = "TestUser";
        #endregion

        #region Test-Data
        #region Expected LongExport Values

        public static string FullExportOfSimpleContentOne = @"<SexyContentData>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>31d93b03-cfb3-483b-8134-e08bbee9cd2c</Guid>
    <Language></Language>
    <Title>Separate Design from Content.</Title>
    <Content>Now you can finally split content and design - any way you want to. If you're a control-freak, you can limit the input possibilities to simple input fields with text and number, but you can also decide to give more freedom with WYSIWYG-fields - but of course only where you want to allow it. &lt;strong&gt;&lt;em&gt;2Sexy Content&lt;/em&gt; &lt;/strong&gt;let's you configure it exactly how you want it.</Content>
    <PreviewContent>Easy and immediate separation of concerns.</PreviewContent>
    <Link>[""""]</Link>
    <Image>/Portals/0/Images/home-bike-vintage.png</Image>
  </Entity>
</SexyContentData>";

        public static string FullExportOfSimpleContentAll = @"<SexyContentData>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>5cba4fab-34d9-408f-a6ef-50ebfdc9dc8e</Guid>
    <Language></Language>
    <Title>Electric Bicycles</Title>
    <Content>&lt;p&gt;Proin condimentum odio ipsum, sit amet consequat lacus. Ut elementum nisl id lectus ullamcorper bibendum. Praesent pellentesque bibendum sodales. Aenean ut convallis velit. In vestibulum aliquam condimentum. Vivamus tincidunt ante id nibh volutpat porta. Nulla ut dolor massa, eu vestibulum massa. Suspendisse hendrerit mi a mi vehicula tempus vitae nec felis. Praesent ac urna at lectus porta porttitor. Cras nulla leo, porta nec laoreet ac, lobortis non enim. Duis nisl nisi, mattis vel egestas sed, venenatis sed turpis. Praesent pellentesque bibendum sodales. Aenean ut convallis velit. In vestibulum aliquam condimentum. Vivamus tincidunt ante id nibh volutpat porta.Praesent pellentesque bibendum sodales. Aenean ut convallis velit. In vestibulum aliquam condimentum. Vivamus tincidunt ante id nibh volutpat porta.&lt;/p&gt;
&lt;p&gt; Proin condimentum odio ipsum, sit amet consequat lacus. Ut elementum nisl id lectus ullamcorper bibendum. Praesent pellentesque bibendum sodales. Aenean ut convallis velit. In vestibulum aliquam condimentum. Vivamus tincidunt ante id nibh volutpat porta.&lt;br /&gt;
&lt;br /&gt;
Nulla ut dolor massa, eu vestibulum massa. Suspendisse hendrerit mi a mi vehicula tempus vitae nec felis. Praesent ac urna at lectus porta porttitor. Cras nulla leo, porta nec laoreet ac, lobortis non enim. Duis nisl nisi, mattis vel egestas sed, venenatis sed turpis.&lt;br /&gt;
&lt;br /&gt;
Praesent pellentesque bibendum sodales. Aenean ut convallis velit. In vestibulum aliquam condimentum. Vivamus tincidunt ante id nibh volutpat porta.Praesent pellentesque bibendum sodales. Aenean ut convallis velit. In vestibulum aliquam condimentum. Vivamus tincidunt ante id nibh volutpat porta.&lt;br /&gt;
&lt;br /&gt;
&lt;/p&gt;</Content>
    <PreviewContent>&lt;p&gt;Proin condimentum odio ipsum, sit amet consequat lacus. Ut elementum nisl id lectus ullamcorper bibendum. Praesent pellentesque bibendum sodales. Aenean ut convallis velit. In vestibulum aliquam condimentum. Vivamus tincidunt ante id nibh volutpat porta. Nulla ut dolor massa, eu vestibulum massa. Suspendisse hendrerit mi a mi vehicula tempus vitae nec felis. Praesent ac urna at lectus porta porttitor. Cras nulla leo, porta nec laoreet ac, lobortis non enim.&lt;br /&gt;
&lt;br /&gt;
&lt;/p&gt;</PreviewContent>
    <Link>[""""]</Link>
    <Image>/Portals/0/Images/bike-performance.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>13720d7e-df00-4a6a-b803-cfcf2ec09618</Guid>
    <Language></Language>
    <Title>Breakthrough Retina display.</Title>
    <Content>The Retina display on the new, third-generation iPad makes everything look crisper and more lifelike. Text is razor sharp. Colors are more vibrant. Photos and videos are rich with detail. All thanks to 3.1 million pixels powered by the new A5X chip. It’s the best mobile display ever.</Content>
    <PreviewContent>It’s even more than meets the eye.</PreviewContent>
    <Link>#</Link>
    <Image>/Portals/0/Images/overview_bucket_retina.jpg</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>bd64156c-ce7e-450b-b1ad-d8d96c838ce5</Guid>
    <Language></Language>
    <Title>Introducin Siri.</Title>
    <Content>Ask Siri to make calls, send texts, set reminders, and more. Just talk the way you talk. Siri understands what you say and knows what you mean.</Content>
    <PreviewContent>The intelligent assistant that's there to help. Just ask.</PreviewContent>
    <Link>#</Link>
    <Image>/Portals/0/Content/Apple/iphone-testimage.jpg</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>e3537679-5ed8-485f-b6be-33775f1d132c</Guid>
    <Language></Language>
    <Title>iCloud.</Title>
    <Content>iCloud stores your music, photos, documents, and more. And wirelessly pushes them to your devices. It’s automatic, effortless, and seamless. And it just works. iCloud is free with iOS 5.</Content>
    <PreviewContent>Your content.
On all your devices.</PreviewContent>
    <Link>#</Link>
    <Image>/Portals/0/Content/Apple/iphone-testimage2.jpg</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>fe30ab23-e111-4cdb-bda8-20690f8a1749</Guid>
    <Language></Language>
    <Title>Introducing 2Sexy-Siri.</Title>
    <Content>Ask Siri to make calls, send texts, set reminders, and more. Just talk the way you talk. Siri understands what you say and knows what you mean.</Content>
    <PreviewContent>The intelligent assistant that's there to help. Just ask.</PreviewContent>
    <Link>#</Link>
    <Image>/Portals/0/Content/Apple/bucket_icon_siri.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>3ead8328-dacb-48cf-9dac-c03150008b54</Guid>
    <Language></Language>
    <Title>Dual-core A5 chip.</Title>
    <Content>Two powerful cores and faster graphics make all the difference when you’re browsing the web, going from app to app to app, gaming, and doing just about everything. </Content>
    <PreviewContent>Even faster everything.</PreviewContent>
    <Link>#</Link>
    <Image>/Portals/0/Content/Apple/bucket_icon_a5.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>d9c83864-6710-4fea-8c65-7b540be8e843</Guid>
    <Language></Language>
    <Title>iSight camera.</Title>
    <Content>What’s more amazing than an 8MP camera with all-new optics that also shoots 1080p HD video? The fact that it’s on a phone.</Content>
    <PreviewContent>It just might be the best camera ever on a phone.</PreviewContent>
    <Link>#</Link>
    <Image>/Portals/0/Content/Apple/bucket_icon_camera.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>a8e883ec-89ad-4b00-b955-b39d9a5bfb14</Guid>
    <Language></Language>
    <Title>iOS 5. </Title>
    <Content>With over 200 new software features, the world’s most advanced mobile operating system is now even further ahead of anything else. </Content>
    <PreviewContent>Years ahead and moving forward. </PreviewContent>
    <Link>#</Link>
    <Image>/Portals/0/Content/Apple/bucket_icon_ios.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>6fa6e500-4ea4-4bd9-ac93-bdd496fe58f2</Guid>
    <Language></Language>
    <Title>iCloud.</Title>
    <Content>iCloud stores your music, photos, documents, and more. And wirelessly pushes them to your devices. It’s automatic, effortless, and seamless. And it just works. iCloud is free with iOS 5.</Content>
    <PreviewContent>Your content.
On all your devices.</PreviewContent>
    <Link>#</Link>
    <Image>/Portals/0/Content/Apple/bucket_icon_icloud.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>42aa061d-ebea-4811-8525-d1ad04812d37</Guid>
    <Language></Language>
    <Title>Breakthrough Retina content.</Title>
    <Content>The Retina display on the new, third-generation iPad makes everything look crisper and more lifelike. Text is razor sharp. Colors are more vibrant. Photos and videos are rich with detail. All thanks to 3.1 million pixels powered by the new A5X chip. It’s the best mobile display ever. </Content>
    <PreviewContent>It’s even more than meets the eye.</PreviewContent>
    <Link>#</Link>
    <Image>/Portals/0/Content/Apple/overview_bucket_retina.jpg</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>9570a1e3-6f15-419c-8790-5b4aaef1c1b0</Guid>
    <Language></Language>
    <Title>5MP iSight camera.</Title>
    <Content>The new iPad features a 5-megapixel iSight camera with advanced optics, a backside illumination sensor, auto white balance, and face detection for incredible still images. And you can record 1080p HD video, too. So every moment you capture looks as great as you remember. </Content>
    <PreviewContent>Take your best shots yet.</PreviewContent>
    <Link>#</Link>
    <Image>/Portals/0/Content/Apple/overview_bucket_camera.jpg</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>4954e8be-4a84-4031-b33d-7fb5375c7397</Guid>
    <Language></Language>
    <Title>Consistent Content Design.</Title>
    <Content>&lt;span&gt;Just the fact that the images align perfectly, are always in proportion and look great! &lt;/span&gt;&lt;span&gt;&lt;em&gt;&lt;strong&gt;2Sexy Content&lt;/strong&gt;&lt;/em&gt; allows &lt;em&gt;web designers&lt;/em&gt; to create perfect templates, so that &lt;em&gt;content editors&lt;/em&gt; can create consistent and professional content. &lt;/span&gt;&lt;span&gt;Comming Up Later: A special feature is also the included image thumbnailer, which will help you ensure that the images are always the right size. (documentation not done yet :)&lt;/span&gt;</Content>
    <PreviewContent>Professional web sites require &lt;strong&gt;consistent content&lt;/strong&gt;.</PreviewContent>
    <Link>[""""]</Link>
    <Image>/Portals/0/Images/home-bike-vintage.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>b485ab28-085b-4279-8dee-13d6e24c7c9a</Guid>
    <Language></Language>
    <Title>Multiple Outputs.</Title>
    <Content>An employee could be shown as a large image with superimposed name Data sheet, or like a Name-Card with Google maps showing where to find him. You could also use the same data to provide a Twitter feed or generating a vCard.</Content>
    <PreviewContent>Create various designed outputs with the same data.</PreviewContent>
    <Link>[""""]</Link>
    <Image>/Portals/0/Images/home-bike-vintage.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>3492e16d-92af-4ff7-bc9c-29e229c12a97</Guid>
    <Language></Language>
    <Title>Complex Content.</Title>
    <Content>&lt;span&gt;Users with normal Word skills don't have the ability to handle HTML, so &lt;em&gt;2Sexy Content&lt;/em&gt; helps the designer create the templates with the appropriate HTML structure, CSS, scripts etc.&lt;/span&gt;</Content>
    <PreviewContent>Great content with sophisticated design.</PreviewContent>
    <Link>[""""]</Link>
    <Image>/Portals/0/Images/home-bike-vintage.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>db4d491a-358d-429e-b7df-15eccc2f896b</Guid>
    <Language></Language>
    <Title>Animated and Interactive.</Title>
    <Content>&lt;span&gt;To really empower content editors, we enable them to create interactive content-elements, or content-elements containing some predefined animation. With &lt;em&gt;2Sexy Content&lt;/em&gt; this is very easy. By preparing the output template (see demos) we simplify the creation of such things, so that anybody can create awesome effects :).&lt;/span&gt;</Content>
    <PreviewContent>Give you editors the ""I really did that!""</PreviewContent>
    <Link>[""""]</Link>
    <Image>/Portals/0/Images/home-bike-vintage.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>22d8e97e-98a1-4bc9-b5c7-863ba8a6b22b</Guid>
    <Language></Language>
    <Title>Animation Configurations.</Title>
    <Content>&lt;span&gt;Got a complex effect or one that you use a lot. Again the power of &lt;em&gt;2Sexy Content&lt;/em&gt; helps you create the configuration-UI within minutes, and unleash the magic of the effect.&lt;/span&gt;</Content>
    <PreviewContent>Instantly provide complex screen candy.</PreviewContent>
    <Link>[""""]</Link>
    <Image>/Portals/0/Images/home-bike-vintage.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>31d93b03-cfb3-483b-8134-e08bbee9cd2c</Guid>
    <Language></Language>
    <Title>Separate Design from Content.</Title>
    <Content>Now you can finally split content and design - any way you want to. If you're a control-freak, you can limit the input possibilities to simple input fields with text and number, but you can also decide to give more freedom with WYSIWYG-fields - but of course only where you want to allow it. &lt;strong&gt;&lt;em&gt;2Sexy Content&lt;/em&gt; &lt;/strong&gt;let's you configure it exactly how you want it.</Content>
    <PreviewContent>Easy and immediate separation of concerns.</PreviewContent>
    <Link>[""""]</Link>
    <Image>/Portals/0/Images/home-bike-vintage.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>2051ced8-e57b-4a88-a030-7e6115f5d2e0</Guid>
    <Language></Language>
    <Title>Parallel Redesign</Title>
    <Content>[""""]</Content>
    <PreviewContent>By separating the actual layout from the content, 2Sexy Content allows to you create content and design it at the same time.</PreviewContent>
    <Link>[""""]</Link>
    <Image>/Portals/0/Content/Apple/overview_ipad_in_education.png</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>f81c5d3f-9c83-4c2e-ac38-d1a4aef7cc60</Guid>
    <Language></Language>
    <Title>2Sexy Content</Title>
    <Content>[""""]</Content>
    <PreviewContent>Making beatiful web sites possible. Screen-Candy included.</PreviewContent>
    <Link>[""""]</Link>
    <Image>/Portals/0/Content/Apple/overview_bucket_retina.jpg</Image>
  </Entity>
  <Entity Type=""BasicContentwithPreviewandImagebuiltin"">
    <Guid>502cc57c-9030-4e6c-a1a6-fc2be24bb417</Guid>
    <Language></Language>
    <Title>Instant Prototyping</Title>
    <Content>[""""]</Content>
    <PreviewContent>Create page prototypes within seconds, assembling templates with prefilled content.</PreviewContent>
    <Link>[""""]</Link>
    <Image>/Portals/0/Content/Apple/overview_ipad_in_education.png</Image>
  </Entity>
</SexyContentData>";
        #endregion

        #endregion

        [ClassInitialize]
        public static void InitUnity(TestContext tc)
        {
            var cont = Eav.Factory.Container;
            cont.RegisterType(typeof(IEavValueConverter), typeof(NeutralValueConverter), new InjectionConstructor());
            cont.RegisterType(typeof(IEavUserInformation), typeof(NeutralEavUserInformation), new InjectionConstructor());
            
        }


        //[TestMethod]
        //[Ignore]
        //public void XmlImport_ImportOneCorrectly()
        //{
            
        //    throw new Exception("This test can't pass yet - had to turn off import because it generated too much data - ask 2dm");
        //    // todo steps
        //    // 1. Get original
        //    // 2. Import one
        //    // 3. Ensure the import was correct
        //    // 4. Re-export and compare with import file

        //    var db = EavDataController.Instance(ZoneId, AppId);
        //    var dbEntity = db.Entities.GetEntitiesByGuid(SingleItemGuid).First();

        //    // Assert.AreEqual(5, dbEntity.Values.Count);

        //    var importer = new XmlImport(ZoneId, AppId, ContentTypeSimpleContent,
        //        GenerateStreamFromString(FullExportOfSimpleContentOne), new List<string>(), "", EntityClearImport.None,
        //        ResourceReferenceImport.Keep);

        //    var entity31d9 = importer.Entities.First(e => e.EntityGuid == SingleItemGuid);
        //    var previewContent = entity31d9.Values["PreviewContent"][0];

        //    Assert.AreEqual(5, entity31d9.Values.Count);

        //    Assert.AreEqual("Easy and immediate separation of concerns.", previewContent.StringValueForTesting);

            
        //    // importer.PersistImportToRepository(UserNameOfTestingScript);
        //}

        [TestMethod]
        public void XmlImport_Import5XmlWith20ContentItems()
        {
            //for (var x = 0; x < 5; x++)
            //     Do1ImportWith20ContentItems();

            // Notes: run 5x 2015-06-21 before optimizations: 
            // Run 1: 9.14 seconds
            // Run 2: 9.37 seconds
            // Run 3: 9.73 seconds
            // Run 4: 9.51 seconds
            // Run 5: 9.76 seconds
        }

        private void Do1ImportWith20ContentItems()
        {
            var fileImport = new XmlImport(ZoneId, AppId, ContentTypeSimpleContent,
                GenerateStreamFromString(FullExportOfSimpleContentAll), new List<string>(), "", EntityClearImport.None,
                ResourceReferenceImport.Keep);

            var entity31d9 = fileImport.Entities.First(e => e.EntityGuid == new Guid("31d93b03-cfb3-483b-8134-e08bbee9cd2c"));
            var previewContent = entity31d9.Values["PreviewContent"][0];
            Assert.AreEqual("Easy and immediate separation of concerns.", (previewContent as IImpValue).ToString());

            throw new Exception("Cant finish testing import yet - there is still a large bug creating much too much data!");
            //var fileImported = fileImport.PersistImportToRepository(UserNameOfTestingScript);
        }



        public Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
