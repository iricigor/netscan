namespace NetScan.Services;

public static class MacVendorLookup
{
    // Key = first 6 hex chars of MAC (uppercase, no separators)
    private static readonly Dictionary<string, string> _oui = new(StringComparer.OrdinalIgnoreCase)
    {
        // Apple
        {"001AE3","Apple"}, {"001B63","Apple"}, {"001CB3","Apple"}, {"001D4F","Apple"},
        {"001E52","Apple"}, {"001F5B","Apple"}, {"001FF3","Apple"}, {"0021E9","Apple"},
        {"002241","Apple"}, {"002312","Apple"}, {"00233C","Apple"}, {"00236C","Apple"},
        {"0023DF","Apple"}, {"002436","Apple"}, {"002500","Apple"}, {"00254B","Apple"},
        {"0025BC","Apple"}, {"002608","Apple"}, {"00264A","Apple"}, {"0026B0","Apple"},
        {"0026BB","Apple"}, {"003EE1","Apple"}, {"0050E4","Apple"}, {"006171","Apple"},
        {"040CCE","Apple"}, {"041552","Apple"}, {"041E64","Apple"}, {"042665","Apple"},
        {"0452F3","Apple"}, {"045453","Apple"}, {"0469F8","Apple"}, {"04D3CF","Apple"},
        {"0C3E9F","Apple"}, {"0C4DE9","Apple"}, {"0C74C2","Apple"}, {"0C771A","Apple"},
        {"0CBC9F","Apple"}, {"101C0C","Apple"}, {"1040F3","Apple"}, {"1093E9","Apple"},
        {"189EFC","Apple"}, {"18AF61","Apple"}, {"18E7F4","Apple"}, {"246AB8","Apple"},
        {"38F9D3","Apple"}, {"3C0754","Apple"}, {"40CBC0","Apple"}, {"70DEE2","Apple"},
        {"AC3C0B","Apple"}, {"ACFDEC","Apple"}, {"B8E856","Apple"}, {"D0E140","Apple"},

        // Samsung
        {"001632","Samsung"}, {"001D25","Samsung"}, {"001E7D","Samsung"}, {"001FCC","Samsung"},
        {"002407","Samsung"}, {"0024E9","Samsung"}, {"002566","Samsung"}, {"0026E2","Samsung"},
        {"002706","Samsung"}, {"00E3B2","Samsung"}, {"089860","Samsung"}, {"0C8910","Samsung"},
        {"1072C5","Samsung"}, {"107EC6","Samsung"}, {"10D542","Samsung"}, {"141AA3","Samsung"},
        {"1816C9","Samsung"}, {"1C5A3E","Samsung"}, {"1C66AA","Samsung"}, {"200DB0","Samsung"},
        {"2C44FD","Samsung"}, {"30196F","Samsung"}, {"3013E2","Samsung"}, {"380195","Samsung"},
        {"3C8BFE","Samsung"}, {"404E36","Samsung"}, {"445C36","Samsung"}, {"4844F7","Samsung"},
        {"502B73","Samsung"}, {"5C4974","Samsung"}, {"5CD2E8","Samsung"}, {"640980","Samsung"},
        {"6C2F2C","Samsung"}, {"6CAB31","Samsung"}, {"74458A","Samsung"}, {"7825AD","Samsung"},
        {"7C0BC6","Samsung"}, {"848177","Samsung"}, {"84119E","Samsung"}, {"889B39","Samsung"},
        {"8C71F8","Samsung"}, {"8C8590","Samsung"}, {"98522B","Samsung"}, {"A4E3F0","Samsung"},
        {"B8D9CE","Samsung"}, {"BC765E","Samsung"}, {"C06333","Samsung"}, {"C4731E","Samsung"},
        {"CC07AB","Samsung"}, {"D0175A","Samsung"}, {"E4E0C5","Samsung"}, {"E87D76","Samsung"},
        {"EC1F72","Samsung"}, {"F008F1","Samsung"}, {"F4428F","Samsung"}, {"FC1F19","Samsung"},

        // Google / Nest
        {"001A11","Google"}, {"3C5AB4","Google/Chromecast"}, {"6C5AB5","Google/Chromecast"},
        {"F4F5D8","Google/Nest"}, {"18B430","Google/Nest"}, {"844067","Google/Nest"},
        {"A4DA22","Google"}, {"DA4D6A","Google"},

        // Amazon / Kindle / Echo
        {"0017E5","Amazon"}, {"0CF3EE","Amazon/Echo"}, {"44650D","Amazon/Echo"},
        {"68370E","Amazon"}, {"74C246","Amazon/Echo"}, {"A002DC","Amazon"},
        {"AC63BE","Amazon/Kindle"}, {"B47C9C","Amazon/Echo"}, {"FC65DE","Amazon"},
        {"747548","Amazon/Fire"}, {"F0272D","Amazon"},

        // Cisco
        {"000142","Cisco"}, {"0001C7","Cisco"}, {"000243","Cisco"}, {"0003E3","Cisco"},
        {"0004DD","Cisco"}, {"000581","Cisco"}, {"0006F6","Cisco"}, {"000702","Cisco"},
        {"00072C","Cisco"}, {"00508A","Cisco"}, {"0800A3","Cisco"}, {"001A2F","Cisco"},
        {"001B54","Cisco"}, {"001C57","Cisco"}, {"001C58","Cisco"}, {"001D70","Cisco"},
        {"2C3124","Cisco"}, {"3C0859","Cisco"}, {"6400F1","Cisco"}, {"7069CA","Cisco"},
        {"A0CF5B","Cisco"}, {"B8386C","Cisco"}, {"D46433","Cisco"}, {"E88D28","Cisco"},

        // Intel (Wi-Fi chips)
        {"001517","Intel"}, {"001B21","Intel"}, {"001E67","Intel"}, {"002196","Intel"},
        {"00224D","Intel"}, {"002354","Intel"}, {"002369","Intel"}, {"0024D7","Intel"},
        {"002564","Intel"}, {"10027B","Intel"}, {"10F48B","Intel"}, {"1C6F65","Intel"},
        {"246078","Intel"}, {"287FEF","Intel"}, {"3085A9","Intel"}, {"3C970E","Intel"},
        {"40A5EF","Intel"}, {"48518D","Intel"}, {"549BED","Intel"}, {"5CF7E6","Intel"},
        {"6C2904","Intel"}, {"6CF049","Intel"}, {"744B4B","Intel"}, {"78924B","Intel"},
        {"7C7A91","Intel"}, {"803786","Intel"}, {"84A9C4","Intel"}, {"887018","Intel"},
        {"8C8D28","Intel"}, {"905C44","Intel"}, {"A03E6B","Intel"}, {"AC7BA1","Intel"},
        {"B0A4E4","Intel"}, {"B416F5","Intel"}, {"C8D9D2","Intel"}, {"D85D4C","Intel"},
        {"E89A8F","Intel"}, {"EC086B","Intel"}, {"F40E11","Intel"},

        // TP-Link
        {"001E8C","TP-Link"}, {"002268","TP-Link"}, {"0025D6","TP-Link"}, {"105BED","TP-Link"},
        {"14CC20","TP-Link"}, {"1C3BF3","TP-Link"}, {"204E7F","TP-Link"}, {"28EE52","TP-Link"},
        {"2C4D54","TP-Link"}, {"305A3A","TP-Link"}, {"40A5EF","TP-Link"}, {"50C7BF","TP-Link"},
        {"549CF5","TP-Link"}, {"64709B","TP-Link"}, {"6C72E7","TP-Link"}, {"70627D","TP-Link"},
        {"741AA4","TP-Link"}, {"74DA38","TP-Link"}, {"7886D9","TP-Link"}, {"80EA96","TP-Link"},
        {"843835","TP-Link"}, {"88DCBA","TP-Link"}, {"90F652","TP-Link"}, {"98DAFF","TP-Link"},
        {"A0F3C1","TP-Link"}, {"B0487A","TP-Link"}, {"B4B0240","TP-Link"}, {"C006C3","TP-Link"},
        {"C46E1F","TP-Link"}, {"D86CE9","TP-Link"}, {"DC09C4","TP-Link"}, {"E849DA","TP-Link"},
        {"F09FC2","TP-Link"}, {"F44269","TP-Link"}, {"F81A67","TP-Link"}, {"FC75B7","TP-Link"},

        // Netgear
        {"001B2F","Netgear"}, {"001E2A","Netgear"}, {"001F33","Netgear"}, {"002169","Netgear"},
        {"00224B","Netgear"}, {"0026F2","Netgear"}, {"0846FE","Netgear"}, {"1803E7","Netgear"},
        {"20E52A","Netgear"}, {"28C68E","Netgear"}, {"2C3033","Netgear"}, {"30469A","Netgear"},
        {"44940C","Netgear"}, {"4C60DE","Netgear"}, {"6480C0","Netgear"}, {"6CB0CE","Netgear"},
        {"74441A","Netgear"}, {"84188E","Netgear"}, {"9C3DCF","Netgear"}, {"A040A0","Netgear"},
        {"A42BB0","Netgear"}, {"C03F0E","Netgear"}, {"C4E984","Netgear"}, {"D4CA6D","Netgear"},
        {"E0469A","Netgear"}, {"E0914F","Netgear"}, {"E4F4C6","Netgear"}, {"F83403","Netgear"},

        // Raspberry Pi Foundation
        {"B827EB","Raspberry Pi"}, {"DCA632","Raspberry Pi"}, {"E45F01","Raspberry Pi"},

        // Ubiquiti
        {"002722","Ubiquiti"}, {"04189A","Ubiquiti"}, {"0418D6","Ubiquiti"}, {"18E829","Ubiquiti"},
        {"24A43C","Ubiquiti"}, {"44D9E7","Ubiquiti"}, {"68722D","Ubiquiti"}, {"788A20","Ubiquiti"},
        {"AABBCC","Ubiquiti"}, {"DC9FDB","Ubiquiti"}, {"E063DA","Ubiquiti"}, {"F09FC2","Ubiquiti"},

        // Microsoft / Xbox / Surface
        {"001DD8","Microsoft"}, {"0017FA","Microsoft"}, {"001422","Microsoft"},
        {"28186D","Microsoft"}, {"485B39","Microsoft"}, {"7C1E52","Microsoft"},
        {"98521D","Microsoft"}, {"C8B36D","Microsoft"}, {"7C1A67","Microsoft"},

        // Sony / PlayStation
        {"001315","Sony"}, {"001A80","Sony"}, {"001DE0","Sony"}, {"002231","Sony"},
        {"00247E","Sony"}, {"0024BE","Sony"}, {"0025E7","Sony"}, {"002611","Sony"},
        {"00D9D1","Sony"}, {"709B74","Sony/PlayStation"}, {"A8E3EE","Sony/PlayStation"},
        {"F8461C","Sony"}, {"FC0FE6","Sony"},

        // Nintendo
        {"001656","Nintendo"}, {"001751","Nintendo"}, {"001FC5","Nintendo"}, {"002444","Nintendo"},
        {"002659","Nintendo"}, {"0009BF","Nintendo"}, {"4C2C55","Nintendo"}, {"98E8FA","Nintendo"},
        {"A438CC","Nintendo"},

        // ASUS
        {"001E8C","ASUS"}, {"002354","ASUS"}, {"0024BE","ASUS"}, {"0026DD","ASUS"},
        {"086A0A","ASUS"}, {"107B44","ASUS"}, {"1C872C","ASUS"}, {"20CF30","ASUS"},
        {"2C4D54","ASUS"}, {"3C97AE","ASUS"}, {"40167E","ASUS"}, {"485BA6","ASUS"},
        {"50465D","ASUS"}, {"5404A6","ASUS"}, {"60A44C","ASUS"}, {"6C4B90","ASUS"},
        {"74D435","ASUS"}, {"88D7F6","ASUS"}, {"9C5C8E","ASUS"}, {"AC220B","ASUS"},
        {"BC9946","ASUS"}, {"C8D719","ASUS"}, {"D8FEE3","ASUS"}, {"E03F49","ASUS"},
        {"F0795938","ASUS"},

        // Dell
        {"001A4B","Dell"}, {"001372","Dell"}, {"001563","Dell"}, {"001EC9","Dell"},
        {"00215A","Dell"}, {"002170","Dell"}, {"002564","Dell"}, {"002655","Dell"},
        {"18A99B","Dell"}, {"1C40AF","Dell"}, {"24B6FD","Dell"}, {"3440B5","Dell"},
        {"3C2C30","Dell"}, {"5C260A","Dell"}, {"788CB5","Dell"}, {"848BCD","Dell"},
        {"9840BB","Dell"}, {"A4BADB","Dell"}, {"B083FE","Dell"}, {"BC3065","Dell"},
        {"C81F66","Dell"}, {"D4BE00","Dell"}, {"F04DA2","Dell"}, {"F48E92","Dell"},

        // Lenovo
        {"001D72","Lenovo"}, {"0021CC","Lenovo"}, {"0023AE","Lenovo"}, {"002369","Lenovo"},
        {"10659F","Lenovo"}, {"104FA8","Lenovo"}, {"1C69C4","Lenovo"}, {"3CAB8E","Lenovo"},
        {"4C802D","Lenovo"}, {"54EEA8","Lenovo"}, {"600900","Lenovo"}, {"60D9C7","Lenovo"},
        {"70728B","Lenovo"}, {"742B62","Lenovo"}, {"781C9E","Lenovo"}, {"84FDD1","Lenovo"},
        {"98140D","Lenovo"}, {"A489E8","Lenovo"}, {"C850E3","Lenovo"}, {"D4AEBDE","Lenovo"},

        // HP
        {"001A4B","HP"}, {"001635","HP"}, {"001C2E","HP"}, {"001E0B","HP"},
        {"001F29","HP"}, {"00215A","HP"}, {"00227D","HP"}, {"00248C","HP"},
        {"18A905","HP"}, {"1CC1DE","HP"}, {"28924A","HP"}, {"3085A9","HP"},
        {"3C4A92","HP"}, {"40B034","HP"}, {"5C8A38","HP"}, {"6CC217","HP"},
        {"708BCD","HP"}, {"80C16E","HP"}, {"9457A5","HP"}, {"A0D3C1","HP"},
        {"B499BA","HP"}, {"D4AE52","HP"}, {"EC9A74","HP"}, {"F4CE46","HP"},

        // D-Link
        {"001195","D-Link"}, {"001346","D-Link"}, {"0015E9","D-Link"}, {"001CF0","D-Link"},
        {"00215D","D-Link"}, {"00265A","D-Link"}, {"14D64D","D-Link"}, {"1C7EE5","D-Link"},
        {"28107B","D-Link"}, {"2CC26E","D-Link"}, {"34A84E","D-Link"}, {"40F201","D-Link"},
        {"4EDF39","D-Link"}, {"5401D2","D-Link"}, {"6045F7","D-Link"}, {"78321B","D-Link"},
        {"84C9B2","D-Link"}, {"90946E","D-Link"}, {"A0AB1B","D-Link"}, {"ACC15C","D-Link"},
        {"BCAEC5","D-Link"}, {"C0A0BB","D-Link"}, {"C8BE19","D-Link"}, {"CCB255","D-Link"},

        // Philips Hue / Signify
        {"001788","Philips/Hue"}, {"00178D","Philips"}, {"ECB5FA","Philips/Hue"},

        // Xiaomi
        {"001D80","Xiaomi"}, {"0C1DAF","Xiaomi"}, {"14F65A","Xiaomi"}, {"18598B","Xiaomi"},
        {"28E31F","Xiaomi"}, {"34CE00","Xiaomi"}, {"38A4ED","Xiaomi"}, {"5C2E59","Xiaomi"},
        {"642737","Xiaomi"}, {"64B473","Xiaomi"}, {"6C96C7","Xiaomi"}, {"74519A","Xiaomi"},
        {"7811DC","Xiaomi"}, {"8C97EA","Xiaomi"}, {"9C99A0","Xiaomi"}, {"A086C6","Xiaomi"},
        {"AC2B6E","Xiaomi"}, {"B0E235","Xiaomi"}, {"C46099","Xiaomi"}, {"D4970B","Xiaomi"},
        {"EC40F3","Xiaomi"}, {"F0B429","Xiaomi"}, {"F48B32","Xiaomi"}, {"FC64BA","Xiaomi"},

        // Synology
        {"001132","Synology"}, {"0011322","Synology"}, {"0C731B","Synology"},
        {"90092B","Synology"}, {"BC5FF4","Synology"},

        // QNAP
        {"244BFE","QNAP"}, {"000050","QNAP"},

        // Sonos
        {"000E58","Sonos"}, {"34170A","Sonos"}, {"5CAABB","Sonos"}, {"78280B","Sonos"},
        {"94903A","Sonos"}, {"B8E937","Sonos"},

        // Roku
        {"086DAF","Roku"}, {"0888EF","Roku"}, {"ACF7F3","Roku"}, {"B09B7A","Roku"},
        {"CC6D A0","Roku"}, {"D0A4B8","Roku"}, {"D85D4B","Roku"}, {"DC3A5E","Roku"},
    };

    public static string Lookup(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress) || macAddress == "N/A")
            return "Unknown";

        // Normalize: remove separators and uppercase
        var clean = macAddress.Replace(":", "").Replace("-", "").ToUpperInvariant();
        if (clean.Length < 6) return "Unknown";

        var oui = clean[..6];
        return _oui.TryGetValue(oui, out var vendor) ? vendor : "Unknown";
    }
}
