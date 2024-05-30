using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushToMeowMod
{
	public class MeowMeowOptions : OptionInterface
	{
		public readonly Configurable<bool> AltRivuletSounds;

		public MeowMeowOptions(PushToMeowMain plugin)
        {
			AltRivuletSounds = config.Bind("altrivulet", false);
		}

        public override void Initialize()
        {
			OpTab opTab = new OpTab(this, "Options");
			Tabs = new OpTab[] { opTab };

			UIelement[] opts = new UIelement[]
			{
				new OpLabel(10f, 550f, "Options", true),
				new OpLabel(50f, 500f, "Use alternate sounds for Rivulet (disabled = watery sounds, enabled = high-pitch-y sounds)", false),
				new OpCheckBox(AltRivuletSounds, 10f, 500f)
			};

			opTab.AddItems(opts);
        }
    }
}
