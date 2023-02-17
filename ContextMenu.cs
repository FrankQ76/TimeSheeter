using System.Windows.Forms;

namespace TimeSheeter
{
    public static class ContextMenu
    {

        public static void SetContextMenu(ContextMenuStrip contextMenu)
        {
            contextMenu.Items.Clear();
            
            contextMenu.Items.Add("Toggle(on/off)", null, (s, e) => { Program.ToggleTimeSheet(); });
            
            contextMenu.Items.Add("Exit", null, (s, e) => { System.Windows.Forms.Application.Exit(); });


        }
    }
}