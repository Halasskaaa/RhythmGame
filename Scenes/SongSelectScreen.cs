using System;
using System.Diagnostics;
using System.IO;
using wah.Chart.SSC;

namespace wah.Scenes
{
    internal class SongSelectScreen : IScene
    {
        // TODO: actually list charts
        public void OnDrawFrame(TimeSpan deltaTime, ref WindowRenderer renderer)
        {
            //Load(new ChartPreview
            //{
            //    ChartPath = new("../../../test/DDDimocratic AAAnnihilation//DDDimocratic AAAnnihilation//Idol.ssc"),
            //    AudioPath = new("../../../test/DDDimocratic AAAnnihilation//DDDimocratic AAAnnihilation//Idol.ogg"),
            //    PreviewStart = 0,
            //    PreviewEnd = 1,
            //});
            
            // test
            SSCSimfile simfile;
            var ok = new SSCParser(File.ReadAllText("../../../test/DDDimocratic AAAnnihilation/DDDimocratic AAAnnihilation/Idol/Idol.ssc"),
                                   new DirectoryInfo("../../../test/DDDimocratic AAAnnihilation/DDDimocratic AAAnnihilation/Idol"))
               .Parse(out simfile);
            
            Debug.Assert(ok, "failed to parse chart");


            SceneManager.Current = new PlayerScene(simfile, 0);
        }

        public void OnInput(in InputEvent input)
        {
        }

        //private void Load(ChartPreview preview) {
        //    SceneManager.Current = new ChartPlayerScreen(new(preview));
        //}
    }
}
