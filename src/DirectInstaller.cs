using System;
using System.IO;
using System.Text;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;

class DirectInstaller {
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool MoveFileEx(string source,string target,int flags);
 static void Replace(string temp,string target){if(!MoveFileEx(temp,target,1|8))throw new Win32Exception(Marshal.GetLastWin32Error());}
 const string BackupDir=".ninjin-arabic-direct-backup";
 static byte[] SourceHash,TargetHash;
 static long TargetSize;static int Commands;
 static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;return true;}
 static byte[] Hash(string path){using(var s=File.OpenRead(path))using(var h=SHA256.Create())return h.ComputeHash(s);}
 static string Hex(byte[] b){return BitConverter.ToString(b).Replace("-","").ToLowerInvariant();}
 static BinaryReader Payload(){return new BinaryReader(new GZipStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("patch"),CompressionMode.Decompress));}
 static void Header(BinaryReader r){if(Encoding.ASCII.GetString(r.ReadBytes(8))!="NJARDEL1")throw new Exception("Invalid patch.");SourceHash=r.ReadBytes(32);TargetHash=r.ReadBytes(32);TargetSize=r.ReadInt64();Commands=r.ReadInt32();if(SourceHash.Length!=32||TargetHash.Length!=32||TargetSize<1||Commands<1)throw new Exception("Invalid patch header.");}
 static string Data(string game){var path=Path.Combine(Path.GetFullPath(game),"NinjinClashofCarrots_Data");if(!File.Exists(Path.Combine(path,"resources.assets")))throw new Exception("اختر مجلد لعبة Ninjin الصحيح.");return path;}
 static void Closed(){if(Process.GetProcessesByName("NinjinClashofCarrots").Length!=0)throw new Exception("أغلق اللعبة أولًا ثم أعد المحاولة.");}
 static void CopyBytes(Stream source,Stream dest,long length){var b=new byte[65536];while(length>0){int n=source.Read(b,0,(int)Math.Min(b.Length,length));if(n==0)throw new EndOfStreamException();dest.Write(b,0,n);length-=n;}}
 static void Restore(string source,string target){string temp=target+".arabic-"+Guid.NewGuid().ToString("N")+".tmp";try{File.Copy(source,temp);if(!Equal(Hash(source),Hash(temp)))throw new Exception("فشل التحقق من النسخة.");Replace(temp,target);}finally{if(File.Exists(temp))File.Delete(temp);}}
 static void Install(string game){
  Closed();string data=Data(game),target=Path.Combine(data,"resources.assets"),backup=Path.Combine(data,BackupDir),original=Path.Combine(backup,"resources.assets"),temp=target+".arabic-"+Guid.NewGuid().ToString("N")+".tmp";
  byte[] before=Hash(target);
  if(Equal(before,TargetHash)){Message("التعريب مثبت بالفعل.");return;}
  if(!Equal(before,SourceHash))throw new Exception("ملف اللعبة غير مطابق أو معدّل بمود آخر. استعد ملف Steam الأصلي أولًا. لم يُعدّل الملف.");
  Directory.CreateDirectory(backup);
  if(File.Exists(original)){if(!Equal(Hash(original),SourceHash))throw new Exception("النسخة الاحتياطية غير مطابقة. حُفظت دون تغيير.");}else{File.Copy(target,original);if(!Equal(Hash(original),SourceHash))throw new Exception("فشل التحقق من النسخة الاحتياطية.");}
  try{
   using(var r=Payload()){Header(r);using(var src=File.OpenRead(original))using(var dest=new FileStream(temp,FileMode.CreateNew,FileAccess.Write)){
    for(int i=0;i<Commands;i++){byte kind=r.ReadByte();if(kind==0){long length=r.ReadInt64();if(length<0||dest.Position+length>TargetSize)throw new Exception("Invalid literal size.");CopyBytes(r.BaseStream,dest,length);}else if(kind==1){long start=r.ReadInt64(),length=r.ReadInt64();if(start<0||length<0||start>src.Length-length||dest.Position+length>TargetSize)throw new Exception("Invalid source range.");src.Position=start;CopyBytes(src,dest,length);}else throw new Exception("Invalid patch command.");}
    if(dest.Position!=TargetSize||r.BaseStream.ReadByte()!=-1)throw new Exception("Patch length mismatch.");dest.Flush(true);
   }}
   if(!Equal(Hash(temp),TargetHash))throw new Exception("فشل التحقق من ملف التعريب؛ الأصل لم يتغير.");
   Closed();if(!Equal(Hash(target),before))throw new Exception("تغير ملف اللعبة أثناء التنفيذ. أعد المحاولة.");
   Replace(temp,target);
   File.WriteAllText(Path.Combine(backup,"installed.txt"),"source="+Hex(SourceHash)+"\r\ntarget="+Hex(TargetHash)+"\r\n",Encoding.UTF8);
   Message("تم التثبيت. اختر English داخل اللعبة لظهور العربية. النسخة الاحتياطية محفوظة.");
  }finally{if(File.Exists(temp))File.Delete(temp);}
 }
 static void Uninstall(string game){
  Closed();string data=Data(game),target=Path.Combine(data,"resources.assets"),original=Path.Combine(data,BackupDir,"resources.assets");
  if(!File.Exists(original)||!Equal(Hash(original),SourceHash))throw new Exception("لا توجد نسخة احتياطية أصلية موثقة. لم يُعدّل الملف.");
  byte[] current=Hash(target);if(!Equal(current,SourceHash)&&!Equal(current,TargetHash))throw new Exception("تغير ملف اللعبة بعد التثبيت. توقفت الإزالة لحفظ التغييرات الأخرى.");
  Restore(original,target);Message("تمت استعادة الملف الأصلي. النسخة الاحتياطية محفوظة.");
 }
 static bool ConsoleMode;
 static void Message(string value){if(ConsoleMode)Console.WriteLine(value);else MessageBox.Show(value,"تعريب Ninjin",MessageBoxButtons.OK,MessageBoxIcon.Information);}
 [STAThread]static int Main(string[] args){
  try{using(var r=Payload())Header(r);if(args.Length==2){ConsoleMode=true;if(args[0]=="--install")Install(args[1]);else if(args[0]=="--uninstall")Uninstall(args[1]);else throw new Exception("Unknown command.");return 0;}
   Application.EnableVisualStyles();var form=new Form{Text="تعريب Ninjin — التثبيت المباشر",Width=650,Height=280,StartPosition=FormStartPosition.CenterScreen,Font=new Font("Segoe UI",11),RightToLeft=RightToLeft.Yes};
   var title=new Label{Text="اختر مجلد اللعبة ثم ثبّت التعريب",Left=20,Top=15,Width=590,Height=35,TextAlign=ContentAlignment.MiddleCenter};
   var path=new TextBox{Left=20,Top=65,Width=490,Text=@"H:\SteamLibrary\steamapps\common\Ninjin Clash of Carrots",RightToLeft=RightToLeft.No};
   var browse=new Button{Text="اختيار",Left=520,Top=63,Width=90,Height=30};browse.Click+=(s,e)=>{using(var dialog=new FolderBrowserDialog()){dialog.Description="مجلد اللعبة الذي يحتوي NinjinClashofCarrots.exe";if(dialog.ShowDialog()==DialogResult.OK)path.Text=dialog.SelectedPath;}};
   var install=new Button{Text="تثبيت التعريب",Left=330,Top=120,Width=220,Height=42};var remove=new Button{Text="إزالة التعريب",Left=80,Top=120,Width=220,Height=42};
   var note=new Label{Text="أغلق اللعبة أولًا. يُحفظ الأصل تلقائيًا. اختر English بعد التثبيت.",Left=20,Top=180,Width=590,Height=38,TextAlign=ContentAlignment.MiddleCenter};
   Action<bool> run=put=>{install.Enabled=remove.Enabled=browse.Enabled=false;try{if(put)Install(path.Text);else Uninstall(path.Text);}catch(Exception ex){MessageBox.Show(ex.Message,"تعذر التنفيذ",MessageBoxButtons.OK,MessageBoxIcon.Error);}finally{install.Enabled=remove.Enabled=browse.Enabled=true;}};
   install.Click+=(s,e)=>run(true);remove.Click+=(s,e)=>run(false);form.Controls.AddRange(new Control[]{title,path,browse,install,remove,note});Application.Run(form);return 0;
  }catch(Exception ex){if(ConsoleMode)Console.Error.WriteLine(ex.ToString());else MessageBox.Show(ex.Message,"تعذر التنفيذ");return 1;}
 }
}
