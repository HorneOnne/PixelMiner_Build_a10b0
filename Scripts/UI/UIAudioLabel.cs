namespace PixelMiner.UI
{
    public class UIAudioLabel : UISettingsLabel
    {
        public override void Select(bool isSelect)
        {
            base.Select(isSelect);

            UIMenuManager.Instance.DisplaySettingAudioMenu(isSelect);
        }
    }
}
