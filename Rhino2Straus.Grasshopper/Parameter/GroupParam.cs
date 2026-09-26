using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Grasshopper.Datatype;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Rhino2Straus.Grasshopper.Straus.Parameters
{
    public class GroupParam : GH_PersistentParam<GH_ElementGroup>
    {
        protected override System.Drawing.Bitmap Icon => ComponentIcons.Get("Group", ComponentTopics.Model);
        public GroupParam()
            : base("Group", "G", "Contains a collection of groups", ComponentTopics.Tab, ComponentTopics.Model)
        {

        }

        protected override GH_GetterResult Prompt_Plural(ref List<GH_ElementGroup> values)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_ElementGroup value)
        {
            return GH_GetterResult.success;
        }


        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendObjectNameEx(menu);
            Menu_AppendWireDisplay(menu);

            Menu_AppendSeparator(menu);

            Menu_AppendDisconnectWires(menu);

            Menu_AppendSeparator(menu);

            Menu_AppendReverseParameter(menu);
            Menu_AppendFlattenParameter(menu);
            Menu_AppendGraftParameter(menu);
            Menu_AppendSimplifyParameter(menu);

            Menu_AppendSeparator(menu);

            Menu_AppendItem(menu, "Set string separator");
            Menu_AppendTextItem(menu, ElementGroupModel._separator, new GH_MenuTextBox.KeyDownEventHandler(OnGapKeyDown), null, true, 0, true);

            Menu_AppendSeparator(menu);

            Menu_AppendDestroyPersistent(menu);
            Menu_AppendInternaliseData(menu);

            return true;
        }

        protected override GH_ElementGroup PreferredCast(object data)
        {
            if (data is GH_String str)
            {
                string strValue = str.Value;
                var a = strValue.Split(new string[] { ElementGroupModel._separator }, StringSplitOptions.RemoveEmptyEntries);

                GH_ElementGroup group = null;

                for (int i = 0; i < a.Length; i++)
                {
                    if (!string.IsNullOrEmpty(a[i]))
                    {
                        if (i == 0)
                            group = new GH_ElementGroup(new ElementGroupModel(a[i]));
                        else
                            group = new GH_ElementGroup(new ElementGroupModel(a[i], group.Value));
                    }
                }

                return group;
            }
            return base.PreferredCast(data);
        }

        private void OnGapKeyDown(GH_MenuTextBox textBox, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox.Text != null)
                {
                    ExpireSolution(true);
                }
            }
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;


        public override Guid ComponentGuid => new Guid("a41ed71a-1b88-4305-8e0f-ed97c3aa1ed7");
    }
}
