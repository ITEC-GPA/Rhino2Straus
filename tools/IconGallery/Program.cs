using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using Rhino2Straus.Grasshopper;

string root=Path.GetFullPath(args.Length>0?args[0]:".");
var entries=JsonSerializer.Deserialize<Entry[]>(File.ReadAllText(Path.Combine(root,"docs","components.json")))!;
int width=1200,columns=5,cell=240,row=58,top=72;
using var sheet=new Bitmap(width,top+row*((entries.Length+columns-1)/columns)+24);
using var g=Graphics.FromImage(sheet);g.Clear(Color.FromArgb(246,248,250));
using var title=new Font("Segoe UI",20,FontStyle.Bold);using var font=new Font("Segoe UI",9);using var small=new Font("Segoe UI",7);
g.DrawString("Rhino2Straus / Component icons",title,Brushes.Black,18,10);
g.DrawString($"{entries.Length} components · native 24 × 24 pixels · Straus7 R3",font,Brushes.DimGray,20,45);
string assets=Path.Combine(root,"Rhino2Straus.Grasshopper","Assets","Icons");Directory.CreateDirectory(assets);
for(int i=0;i<entries.Length;i++)
{
    var e=entries[i];int x=(i%columns)*cell,y=top+(i/columns)*row;
    var icon=ComponentIcons.Get(e.Name,e.Category);
    if(icon.Width!=24||icon.Height!=24)throw new InvalidDataException("Invalid icon size");
    icon.Save(Path.Combine(assets,e.Class+".png"),ImageFormat.Png);
    if((i/columns)%2==0)g.FillRectangle(Brushes.White,x,y,cell,row);
    g.DrawImageUnscaled(icon,x+10,y+12);
    g.DrawString(e.Name,font,Brushes.Black,new RectangleF(x+42,y+5,190,34));
    g.DrawString(e.Category,small,Brushes.DimGray,x+42,y+39);
}
ComponentIcons.Get("Rhino2Straus","Model").Save(Path.Combine(assets,"Library.png"),ImageFormat.Png);
ComponentIcons.Get("Group","00-Parameter").Save(Path.Combine(assets,"GroupParam.png"),ImageFormat.Png);
string output=Path.Combine(root,"docs","icons.png");sheet.Save(output,ImageFormat.Png);Console.WriteLine($"Rendered {entries.Length+2} icons and {output}");
record Entry(string Class,string Name,string Category,string File);
