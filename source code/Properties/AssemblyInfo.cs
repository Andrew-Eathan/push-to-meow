using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Push To Meow")]
[assembly: AssemblyDescription("makes ur slugcat able to meow meow :3")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("PushToMeowMod")]
[assembly: AssemblyCopyright("Copyright © andreweathan & cioss 2024")]
[assembly: AssemblyTrademark("sussy baka")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("27413039-5d29-4be0-9708-67790da77dfe")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.6.9")]
[assembly: AssemblyFileVersion("1.4.2.0")]

#pragma warning disable CS0618 // SecurityAction.RequestMinimum is obsolete. However, this does not apply to the mod, which still needs it. Suppress the warning indicating that it is obsolete.
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618