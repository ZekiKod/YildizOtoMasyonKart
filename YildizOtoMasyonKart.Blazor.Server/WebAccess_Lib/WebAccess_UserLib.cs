/*
 * Author: İstanbul Yazılım Elektronik Sanayi
 */

using System;
using System.Collections.Specialized;
using System.Text;
using static ASPNetCore_WebAccess.UserLib;

namespace ASPNetCore_WebAccess
{
    public class UserLib
    {
        public const int NO_ID = -1;
        public const int MAX_READER_CAPACITY = 16;
        public const int ZAMAN_KODU_PARCA_SAYISI = 5; // Bir zaman kodunun kaç parçadan oluştuğunu bildirir.
        public const byte PGM_KART_SAYISI = 5;      // PGM(Paylaşımlı Geçiş Modu) modu için kullanıcının grubunda maksimum kaç kişi olabileceğini bildirir.

        public enum AntiPassbackStatus
        {
            Outside = 0, // Dışarıda
            Inside = 1, // İçeride
            None = 2 // Bilinmiyor
        }

        public class UserRelays
        {
            public UserRelays()
            {
                ARelays = new bool[MAX_READER_CAPACITY];
            }

            public bool this[int RelayNum]
            {
                get { return ARelays[RelayNum - 1]; }
                set { ARelays[RelayNum - 1] = value; }
            }

            public Int32 Count()
            {
                return ARelays.Length;
            }

            private bool[] ARelays;
        }

        public class UserPGMInfo
        {
            public UserPGMInfo()
            {
                UserList = new UInt64[PGM_KART_SAYISI];
                Clear();
            }

            public UInt64 this[int Index]
            {
                get { return UserList[Index]; }
                set { UserList[Index] = value; }
            }

            public void Clear()
            {
                AAccessCapacity = 0;

                for (int i = 0; i < PGM_KART_SAYISI; i++)
                    UserList[i] = 0;
            }

            public UInt32 AccessCapacity
            {
                get { return AAccessCapacity; }
                set { AAccessCapacity = value; }
            }

            public Int32 UserCapacity()
            {
                return UserList.Length;
            }

            private UInt32 AAccessCapacity;
            public UInt64[] UserList;
        }

        public class UserHourPeriod
        {
            public UserHourPeriod()
            {
                FBeginTime = new DateTime(1, 1, 1, 0, 0, 0, 0);
                FEndTime = new DateTime(1, 1, 1, 0, 0, 0, 0);
                FAccessLimit = 0;
            }

            public DateTime BeginTime
            {
                get { return FBeginTime; }
                set { FBeginTime = value; }
            }

            public DateTime EndTime
            {
                get { return FEndTime; }
                set { FEndTime = value; }
            }

            public UInt32 AccessLimit
            {
                get { return FAccessLimit; }
                set { FAccessLimit = value; }
            }

            public void SetValue(DateTime Begin_Time, DateTime End_Time, UInt32 Access_Limit)
            {
                FBeginTime = Begin_Time;
                FEndTime = End_Time;
                FAccessLimit = Access_Limit;
            }

            public override string ToString()
            {
                return (FBeginTime.ToString("HH:mm") + "~" + FEndTime.ToString("HH:mm") + " Access Limit: " + FAccessLimit.ToString());
            }

            private DateTime FBeginTime;
            private DateTime FEndTime;
            private UInt32 FAccessLimit;
        }

        public class UserTimeZones
        {
            public UserTimeZones()
            {
                FHourPeriods = new List<UserHourPeriod>();

                for (int i = 0; i < ZAMAN_KODU_PARCA_SAYISI; i++)
                    FHourPeriods.Add(new UserHourPeriod());
            }

            public UserHourPeriod this[Int32 Index]
            {
                get { return FHourPeriods[Index]; }
            }

            public UserHourPeriod Period1
            {
                get { return FHourPeriods[0]; }
            }

            public UserHourPeriod Period2
            {
                get { return FHourPeriods[1]; }
            }

            public UserHourPeriod Period3
            {
                get { return FHourPeriods[2]; }
            }

            public UserHourPeriod Period4
            {
                get { return FHourPeriods[3]; }
            }

            public UserHourPeriod Period5
            {
                get { return FHourPeriods[4]; }
            }

            public Int32 Count()
            {
                return FHourPeriods.Count;
            }

            public override string ToString()
            {
                string RetVal = "";

                for (int i = 0; i < ZAMAN_KODU_PARCA_SAYISI; i++)
                {
                    RetVal += FHourPeriods[i].ToString();
                    if (i < 4)
                        RetVal += ", ";
                }

                return RetVal;
            }

            private List<UserHourPeriod> FHourPeriods;
        }

        public class UserReader
        {
            public class DailyAccessNumber
            {
                public Int32 AccessLimit;
                public Int32 AccessCount;
            }

            public class AccessDays
            {
                public bool Monday = true; // Pazartesi. false= Geçiş Yasak, true= Geçebilir.
                public bool Tuesday = true; // Salı
                public bool Wednesday = true; // Çarşamba
                public bool Thursday = true; // Perşembe
                public bool Friday = true; // Cuma
                public bool Saturday = true; // Cumartesi
                public bool Sunday = true; // Pazar
                public bool Holidays = true; // Yıl içindeki tatil günleri gibi zamanlar özel günlerdir.

                override public string ToString()
                {
                    string RetVal = "";

                    if (Monday)
                        RetVal += ", Pazartesi";

                    if (Tuesday)
                        RetVal += ", Salı";

                    if (Wednesday)
                        RetVal += ", Çarsamba";

                    if (Thursday)
                        RetVal += ", Perşembe";

                    if (Friday)
                        RetVal += ", Cuma";

                    if (Saturday)
                        RetVal += ", Cumartesi";

                    if (Sunday)
                        RetVal += ", Pazar";

                    if (Holidays)
                        RetVal += ", Özel Günler";

                    if (RetVal != "")
                        RetVal = RetVal.Substring(2);

                    return RetVal;
                }
            }

            public enum ReaderDirection { None = 0, InPoint = 1, OutPoint = 2, FreeOutPoint = 3 }; // Okuyucunun yönü. 0= Yönsüz, 1= Giriş Kapısı, 2= Çıkış Kapısı, 3= Serbest Çıkış Kapısı.

            // İki adam kuralı kullanım şekli.
            public enum TwoManRuleUseType
            {
                DependentOnReader = 0, // Reader ayarlarına bağlı. Reader.Settings.Two_Man_Rule == true ise klasik iki adam kuralı uygulanır. false ise devre dışı kalır.
                TwoManRule = 1, // Klasik iki adam kuralını uygular. Kişi kartını okuttuktan sonra bir süre yetkili bir başka kişinin kart okutması beklenir. Okutursa röleyi çekip her ikisi içinde geçti log`u kaydeder.
                DependentOnOne = 2  // Birine bağımlı iki adam kuralını uygular. Kişi kartını okuttuktan sonra bir süre PGM kişilerinden birinin kart okutması beklenir. Okutursa röleyi çekip her ikisi içinde geçti log`u kaydeder. (Öğrenciler ve ziyaretçiler gibi. Öğrenciye veli kartları, ziyaretçiye ise görevli kartları PGM listesine eklenebilir)
            };

            public class UserReaderStatus
            {
                public UserReaderStatus()
                {
                    Reader_Direction = ReaderDirection.None; // Okuyucunun yönü. 0= Yönsüz, 1= Giriş Kapısı, 2= Çıkış Kapısı.
                    Anti_Passback_Status = AntiPassbackStatus.None; // Kullanıcının bu kapıdaki içeride/dışarıda durumu.
                    Disable = true; // Erişim Durumu. 0= Erişime açık, 1= Erişime kapalı.
                    Admin = false;  // Kapıya Erişim Şekli. 0= Normal, 1= Admin
                    Credit = false; // Kullanıcı için bu kapıda kontör kullanımını aktif eder. 0= Kontörsüz, 1= AKontörlü kullanım.
                    ParentGroup_Inside = false; // Kullanıcı parent gruptan giriş yapmış ise değeri true olur. Çıkış yaparsa değeri false olur.
                    Two_Man_Rule_Use_Type = TwoManRuleUseType.DependentOnReader;  // İki adam kuralını uygulama şekli.
                }

                public bool Enabled
                {
                    get { return !Disable; }
                    set { Disable = !value; }
                }

                public ReaderDirection Reader_Direction;        // Okuyucunun yönü. 0= Yönsüz, 1= Giriş Kapısı, 2= Çıkış Kapısı.
                public AntiPassbackStatus Anti_Passback_Status; // Kullanıcının bu kapıdaki içeride/dışarıda durumu.
                public bool Disable;                            // Erişim Durumu. 0= Erişime açık, 1= Erişime kapalı.
                public bool Admin;                              // Kapıya Erişim Şekli. 0= Normal, 1= Admin
                public bool Credit;                             // Kullanıcı için bu kapıda kontör kullanımını aktif eder. 0= Kontörsüz, 1= AKontörlü kullanım.
                public bool ParentGroup_Inside;                 // Kullanıcı parent gruptan giriş yapmış ise değeri true olur. Çıkış yaparsa değeri false olur.
                public TwoManRuleUseType Two_Man_Rule_Use_Type; // İki adam kuralını uygulama şekli. 
            }

            public class UserLifetime
            {
                public UserLifetime()
                {
                    BeginTime = new DateTime(1, 1, 1, 0, 0, 0);
                    EndTime = new DateTime(1, 1, 1, 0, 0, 0);
                    AccessLimit = 0;
                    AccessCount = 0;
                }

                // Okuyucu kullanım süresi bilgileri.
                public DateTime BeginTime; // Başlama zamanı.
                public DateTime EndTime; // Bitiş zamanı.
                public Int32 AccessLimit; // Kullanım süresi içindeki geçiş hakkı.
                public Int32 AccessCount; // Kullanım süresi içindeki geçiş sayısı.
            }

            public UserReader()
            {
                // Her gün için geçiş hakkı.
                Daily_Access_Number = new DailyAccessNumber
                {
                    AccessLimit = 0,
                    AccessCount = 0
                };

                // Okuyucunun kullandığı access grubu id.
                Access_Group = NO_ID;

                // Kullanıcının haftanın hangi günlerinde kapıdan geçebileceğini bildirir.
                Access_Days = new AccessDays();
                Access_Days.Monday = true; // Pazartesi. False= Geçiş Yasak, True= Geçebilir.
                Access_Days.Tuesday = true;
                Access_Days.Wednesday = true;
                Access_Days.Thursday = true;
                Access_Days.Friday = true;
                Access_Days.Saturday = true;
                Access_Days.Sunday = true;
                Access_Days.Holidays = true; // Yıl içindeki tatil günleri gibi zamanlar özel günlerdir.


                // Geçiş yaptıktan sonra buradaki süre kadar geçemez. Buradaki değer dakika olarak saklanır.
                Time_Limit = 0;

                // Okuyucudan en son yapılan geçiş zamanının saniye olarak değeri.
                Last_Access_Time = new DateTime(1, 1, 1, 0, 0, 0);

                Status = new UserReaderStatus();
                Lifetime = new UserLifetime();
                Relays = new UserRelays();
                Time_Zones = new UserTimeZones();
            }

            public DailyAccessNumber Daily_Access_Number;   // Günlük geçiş hakkı.
            public Int32 Access_Group;                      // Okuyucunun kullandığı access grubu id. Cihazın access grupları tablosundaki bir kayıta işaret eder.
            public AccessDays Access_Days;                  // Kullanıcının haftanın hangi günlerinde kapıdan geçebileceğini bildirir.
            public UInt32 Time_Limit;                       // Geçiş yaptıktan sonra buradaki süre kadar geçemez. Buradaki değer dakika olarak saklanır.
            public DateTime Last_Access_Time;               // Okuyucudan en son yapılan geçiş zamanının saniye olarak değeri.
            public UserReaderStatus Status;                 // Kullanıcının kapıyı nasıl kullanacağını belirtir.
            public UserLifetime Lifetime;                   // Okuyucu kullanım süresi bilgisi.
            public UserRelays Relays;                       // Okuyucudan geçiş yapılırken tetiklenecek röleler.
            public UserTimeZones Time_Zones;                // Okuyucuyu kullanım saatleri.
        }

        public class UserReaders
        {
            public UserReaders()
            {
                AReaders = new UserReader[MAX_READER_CAPACITY];
                for (int i = 0; i < MAX_READER_CAPACITY; i++)
                    AReaders[i] = new UserReader();
            }

            public UserReader this[int Index]
            {
                get { return AReaders[Index]; }
                set { AReaders[Index] = value; }
            }

            public UserReader Reader1
            {
                get { return AReaders[0]; }
                set { AReaders[0] = value; }
            }

            public UserReader Reader2
            {
                get { return AReaders[1]; }
                set { AReaders[1] = value; }
            }

            public UserReader Reader3
            {
                get { return AReaders[2]; }
                set { AReaders[2] = value; }
            }

            public UserReader Reader4
            {
                get { return AReaders[3]; }
                set { AReaders[3] = value; }
            }

            public UserReader Reader5
            {
                get { return AReaders[4]; }
                set { AReaders[4] = value; }
            }

            public UserReader Reader6
            {
                get { return AReaders[5]; }
                set { AReaders[5] = value; }
            }

            public UserReader Reader7
            {
                get { return AReaders[6]; }
                set { AReaders[6] = value; }
            }

            public UserReader Reader8
            {
                get { return AReaders[7]; }
                set { AReaders[7] = value; }
            }

            public UserReader Reader9
            {
                get { return AReaders[8]; }
                set { AReaders[8] = value; }
            }

            public UserReader Reader10
            {
                get { return AReaders[9]; }
                set { AReaders[9] = value; }
            }

            public UserReader Reader11
            {
                get { return AReaders[10]; }
                set { AReaders[10] = value; }
            }

            public UserReader Reader12
            {
                get { return AReaders[11]; }
                set { AReaders[11] = value; }
            }

            public UserReader Reader13
            {
                get { return AReaders[12]; }
                set { AReaders[12] = value; }
            }

            public UserReader Reader14
            {
                get { return AReaders[13]; }
                set { AReaders[13] = value; }
            }

            public UserReader Reader15
            {
                get { return AReaders[14]; }
                set { AReaders[14] = value; }
            }

            public UserReader Reader16
            {
                get { return AReaders[15]; }
                set { AReaders[15] = value; }
            }

            public Int32 Count()
            {
                return MAX_READER_CAPACITY;
            }

            private UserReader[] AReaders;
        }

        public class AccessInfo
        {
            public bool Forbidden;

            public AccessInfo()
            {
                Forbidden = false;
            }
        }

        public class Protect
        {
            public Protect()
            {
                Anti_Passback_Status = false;
                Credit = false;
                Daily_Access_Count = false;
                LifeTime_Access_Count = false;
                Last_Access_Time = false;
                // TimePeriod_Access_Info = false;
            }

            public bool Anti_Passback_Status;   // Anti_Passback durum bilgisini koru.
            public bool Credit;                 // Kontörü koru.
            public bool Daily_Access_Count;     // Günlük geçiş sayısını koru.
            public bool LifeTime_Access_Count;  // Okuyucuyu kullanma süresi içindeki geçiş sayısını koru.
            public bool Last_Access_Time;       // Son erişim zamanını koru.
            // public bool TimePeriod_Access_Info; // Zaman kodu geçiş bilgisini koru.
        }

        public class User
        {
            public User()
            {
                Protected = new Protect();
                Kart_Bilgisi = 0;
                Sicil_No = 0;
                ARelays = new UserRelays();
                Gecis_Durumu = new AccessInfo();
                Sifre = 0;
                Kontor = UserLib.User.NO_CREDIT_USAGE;
                Access_Grubu = NO_ID;
                APGMInfo = new UserPGMInfo();
                AReaders = new UserReaders();
                Kullanici_Adi = "";
            }

            public Int16 NoCreditUsage
            {
                get { return NO_CREDIT_USAGE; }
            }

            public UInt64 Card_Info // Kart no.
            {
                get { return Kart_Bilgisi; }

                set
                {
                    if (value > 0) // Kart id geçerli ise
                        Kart_Bilgisi = value;
                }
            }

            public string User_Name // Kullanıcı adı.
            {
                get { return Kullanici_Adi; }
                set { Kullanici_Adi = value; }
            }

            public AccessInfo Access_Info
            {
                get { return Gecis_Durumu; }
                set { Gecis_Durumu = value; }
            }

            public UInt64 Password // Kullanıcı şifresi.
            {
                get { return Sifre; }
                set { Sifre = value; }
            }

            public Int32 Credit // Kontör miktarı.
            {
                get { return Kontor; }
                set { Kontor = value; }
            }

            public Int32 Access_Group // Access gruplarına ait id`ler.
            {
                get { return Access_Grubu; }
                set { Access_Grubu = value; }
            }

            public Int64 Registration_Number
            {
                get { return Sicil_No; }
                set { Sicil_No = value; }
            }

            public UserRelays Relays
            {
                get { return ARelays; }
            }

            public UserPGMInfo PGM_Info
            {
                get { return APGMInfo; }
            }

            public UserReaders Readers
            {
                get { return AReaders; }
            }

            public string AntiPassbackStatusToString()
            {
                string RetVal = "";

                for (int i = 0; i < Readers.Count(); i++)
                {
                    RetVal += ", Reader" + (i + 1).ToString() + " (" + Readers[i].Status.Anti_Passback_Status.ToString() + ")";
                }

                if (RetVal.Length > 1)
                    RetVal = RetVal.Substring(2);

                return RetVal;
            }

            private void DeleteSeperator(ref string S)
            {
                S = S.Trim();
                if (S[S.Length - 1] == ',')
                    S = S.Substring(0, S.Length - 1);
            }

            private void BeginClass(ref string S, string Class_Name)
            {
                S += "\"" + Class_Name + "\":{";
            }

            private void EndClass(ref string S, string Seperator = ",")
            {
                DeleteSeperator(ref S);
                S += "}" + Seperator;
            }

            private void BeginArray(ref string S, string Array_Name)
            {
                S += "\"" + Array_Name + "\":[";
            }

            private void EndArray(ref string S, string Seperator = ",")
            {
                DeleteSeperator(ref S);
                S += "]" + Seperator;
            }

            private void AddString(ref string S, string Str)
            {
                S += "\"" + Str + "\"";
            }

            private void AddKey(ref string S, string Name, string Value, string Seperator = ",")
            {
                S += "\"" + Name + "\":\"" + Value + "\"" + Seperator;

                /*
                if (Name.Length > 0)
                    S += "\"" + Name + "\":";

                if (Value.Length > 0)
                    S += "\"" + Value + "\"";

                S += Seperator;
                */
            }

            public void AddKey(ref string S, string Name, bool Value, string Seperator = ",")
            {
                AddKey(ref S, Name, (Value ? "true" : "false"), Seperator);
            }

            public string ToJSON(Int32 Reader_Count = MAX_READER_CAPACITY)
            {
                string RetVal = "";
                bool B;

                if (Reader_Count > Readers.Count())
                    Reader_Count = Readers.Count();

                BeginClass(ref RetVal, "User");
                AddKey(ref RetVal, "Card_Id", Card_Info.ToString());
                AddKey(ref RetVal, "User_Name", User_Name);
                AddKey(ref RetVal, "Registration_Number", Registration_Number.ToString());
                AddKey(ref RetVal, "Password", Password.ToString());

                if (Access_Group > NO_ID)
                    AddKey(ref RetVal, "Access_Group", Access_Group.ToString());

                if (Credit > NO_CREDIT_USAGE)
                    AddKey(ref RetVal, "Credit", Credit.ToString());
                // else AddKey(ref RetVal, "Credit", "none");


                /* Access_Info */
                BeginClass(ref RetVal, "Access_Info");
                AddKey(ref RetVal, "Forbidden", Access_Info.Forbidden, "");
                EndClass(ref RetVal);   // } Access_Info


                /* Relays */
                B = false;
                for (int RNo = 1; RNo < Relays.Count(); RNo++)
                {
                    if (Relays[RNo])
                    {
                        B = true;
                        break;
                    }
                }
                if (B) // Tetiklenecek röle varsa
                {
                    BeginClass(ref RetVal, "Relays");
                    for (int RNo = 1; RNo <= Relays.Count(); RNo++)
                    {
                        if (Relays[RNo])
                            AddKey(ref RetVal, "Relay" + RNo.ToString(), Relays[RNo].ToString());
                    }
                    EndClass(ref RetVal);   // } Relays
                }


                /* PGM_Info */
                B = false;
                for (int i = 0; i < PGM_Info.UserCapacity(); i++)
                {
                    if (PGM_Info.UserList[i] > 0 && PGM_Info.UserList[i] < WebAccess.Empty_Card) // Geçerli bir kart ise
                    {
                        B = true;
                        break;
                    }
                }
                if (B) // Geçerli kart varsa
                {
                    BeginClass(ref RetVal, "PGM_Info");
                    AddKey(ref RetVal, "AccessCapacity", PGM_Info.AccessCapacity.ToString());
                    BeginArray(ref RetVal, "UserList");
                    for (int i = 0; i < PGM_Info.UserCapacity(); i++)
                    {
                        if (PGM_Info.UserList[i] > 0 && PGM_Info.UserList[i] < WebAccess.Empty_Card) // Geçerli bir kart ise
                            RetVal += PGM_Info.UserList[i].ToString() + ",";
                    }
                    EndArray(ref RetVal);   // ] UserList
                    EndClass(ref RetVal);   // } PGM_Info
                }


                /* Readers */
                B = false;
                for (int RNo = 0; RNo < Reader_Count; RNo++)
                {
                    if (Readers[RNo].Status.Disable == false) // Okuyucu etkin ise
                    {
                        B = true;
                        break;
                    }
                }
                if (B) // Geçerli okuyucu varsa
                {
                    BeginClass(ref RetVal, "Readers");
                    for (int RNo = 0; RNo < Reader_Count; RNo++)
                    {
                        BeginClass(ref RetVal, "Reader" + (RNo + 1).ToString());

                        if (Readers[RNo].Status.Disable == false) // Okuyucu etkin ise
                        {
                            /* Status */
                            BeginClass(ref RetVal, "Status");
                            AddKey(ref RetVal, "Disable", "false");
                            AddKey(ref RetVal, "ReaderDirection", Readers[RNo].Status.Reader_Direction.ToString());
                            AddKey(ref RetVal, "Admin", Readers[RNo].Status.Admin);
                            AddKey(ref RetVal, "ParentGroup_Inside", Readers[RNo].Status.ParentGroup_Inside);
                            AddKey(ref RetVal, "Two_Man_Rule_Use_Type", Readers[RNo].Status.Two_Man_Rule_Use_Type.ToString());

                            if (Protected.Anti_Passback_Status == false) //  Anti_Passback durum bilgisi korunmuyorsa.
                                AddKey(ref RetVal, "AntiPassbackStatus", Readers[RNo].Status.Anti_Passback_Status.ToString());

                            if (Protected.Credit == false) //  Kontör bilgisi korunmuyorsa.
                                AddKey(ref RetVal, "Credit", Readers[RNo].Status.Credit);

                            EndClass(ref RetVal);


                            /* Time_Zones */
                            B = false;
                            for (int i = 0; i < Readers[RNo].Time_Zones.Count(); i++)
                            {
                                if (Readers[RNo].Time_Zones[i].BeginTime != Readers[RNo].Time_Zones[i].EndTime) // Geçerli bir zaman ise
                                {
                                    B = true;
                                    break;
                                }
                            }
                            if (B) // Geçerli zaman varsa
                            {
                                BeginArray(ref RetVal, "Time_Zones");
                                for (int i = 0; i < Readers[RNo].Time_Zones.Count(); i++)
                                {
                                    if (Readers[RNo].Time_Zones[i].BeginTime != Readers[RNo].Time_Zones[i].EndTime) // Geçerli bir zaman ise
                                    {
                                        AddString(ref RetVal,
                                                  Readers[RNo].Time_Zones[i].BeginTime.Hour.ToString() + ":" + Readers[RNo].Time_Zones[i].BeginTime.Minute.ToString() + "~" +
                                                  Readers[RNo].Time_Zones[i].EndTime.Hour.ToString() + ":" + Readers[RNo].Time_Zones[i].EndTime.Minute.ToString() + "-" +
                                                  Readers[RNo].Time_Zones[i].AccessLimit.ToString());

                                        RetVal += ",";
                                    }
                                }
                                EndArray(ref RetVal);   // ] Time_Zones
                            }

                            /* Access_Group */
                            if (Readers[RNo].Access_Group > NO_ID)
                                AddKey(ref RetVal, "Access_Group", Readers[RNo].Access_Group.ToString());

                            /* Time_Limit */
                            if (Readers[RNo].Time_Limit > 0)
                                AddKey(ref RetVal, "Time_Limit", Readers[RNo].Time_Limit.ToString());

                            /* Last_Access_Time */
                            if (Protected.Last_Access_Time == false) // Son erişim zamanını korunmuyorsa.
                                AddKey(ref RetVal, "Last_Access_Time",
                                       Readers[RNo].Last_Access_Time.Year.ToString() + "-" +
                                       Readers[RNo].Last_Access_Time.Month.ToString() + "-" +
                                       Readers[RNo].Last_Access_Time.Day.ToString() + " " +
                                       Readers[RNo].Last_Access_Time.Hour.ToString() + ":" +
                                       Readers[RNo].Last_Access_Time.Minute.ToString() + ":" +
                                       Readers[RNo].Last_Access_Time.Second.ToString()
                                    );

                            /* Lifetime */
                            if (Readers[RNo].Lifetime.BeginTime != Readers[RNo].Lifetime.EndTime)
                            {
                                BeginClass(ref RetVal, "Lifetime");

                                AddKey(ref RetVal, "BeginTime",
                                       Readers[RNo].Lifetime.BeginTime.Year.ToString() + "-" +
                                       Readers[RNo].Lifetime.BeginTime.Month.ToString() + "-" +
                                       Readers[RNo].Lifetime.BeginTime.Day.ToString() + " " +
                                       Readers[RNo].Lifetime.BeginTime.Hour.ToString() + ":" +
                                       Readers[RNo].Lifetime.BeginTime.Minute.ToString() + ":00"
                                    );

                                AddKey(ref RetVal, "EndTime",
                                       Readers[RNo].Lifetime.EndTime.Year.ToString() + "-" +
                                       Readers[RNo].Lifetime.EndTime.Month.ToString() + "-" +
                                       Readers[RNo].Lifetime.EndTime.Day.ToString() + " " +
                                       Readers[RNo].Lifetime.EndTime.Hour.ToString() + ":" +
                                       Readers[RNo].Lifetime.EndTime.Minute.ToString() + ":00"
                                    );

                                AddKey(ref RetVal, "AccessLimit", Readers[RNo].Lifetime.AccessLimit.ToString());

                                if (Protected.LifeTime_Access_Count == false) // Okuyucuyu kullanma süresi içindeki geçiş sayısı korunmuyorsa.
                                    AddKey(ref RetVal, "AccessCount", Readers[RNo].Lifetime.AccessCount.ToString());

                                EndClass(ref RetVal);
                            }


                            /* Daily_Access_Number */
                            if (Readers[RNo].Daily_Access_Number.AccessLimit > 0)
                            {
                                BeginClass(ref RetVal, "Daily_Access_Number");
                                AddKey(ref RetVal, "AccessLimit", Readers[RNo].Daily_Access_Number.AccessLimit.ToString());

                                if (Protected.Daily_Access_Count == false) // Günlük geçiş sayısı korunmuyorsa.
                                    AddKey(ref RetVal, "AccessCount", Readers[RNo].Daily_Access_Number.AccessCount.ToString());

                                EndClass(ref RetVal);
                            }


                            /* Access_Days */
                            if (!Readers[RNo].Access_Days.Monday || !Readers[RNo].Access_Days.Tuesday || !Readers[RNo].Access_Days.Wednesday || !Readers[RNo].Access_Days.Thursday ||
                                !Readers[RNo].Access_Days.Friday || !Readers[RNo].Access_Days.Saturday || !Readers[RNo].Access_Days.Sunday)
                            {
                                BeginClass(ref RetVal, "Access_Days");

                                if (!Readers[RNo].Access_Days.Monday)
                                    AddKey(ref RetVal, "Monday", "false");

                                if (!Readers[RNo].Access_Days.Tuesday)
                                    AddKey(ref RetVal, "Tuesday", "false");

                                if (!Readers[RNo].Access_Days.Wednesday)
                                    AddKey(ref RetVal, "Wednesday", "false");

                                if (!Readers[RNo].Access_Days.Thursday)
                                    AddKey(ref RetVal, "Thursday", "false");

                                if (!Readers[RNo].Access_Days.Friday)
                                    AddKey(ref RetVal, "Friday", "false");

                                if (!Readers[RNo].Access_Days.Saturday)
                                    AddKey(ref RetVal, "Saturday", "false");

                                if (!Readers[RNo].Access_Days.Sunday)
                                    AddKey(ref RetVal, "Sunday", "false");

                                EndClass(ref RetVal);
                            }


                            /* Relays */
                            B = false;
                            for (int RL_No = 1; RL_No <= Readers[RNo].Relays.Count(); RL_No++)
                            {
                                if (Readers[RNo].Relays[RL_No]) // Röle tetiklenecekse
                                {
                                    B = true;
                                    break;
                                }
                            }
                            if (B) // Tetiklenecek röle varsa
                            {
                                BeginClass(ref RetVal, "Relays");

                                for (int RL_No = 1; RL_No <= Readers[RNo].Relays.Count(); RL_No++)
                                    if (Readers[RNo].Relays[RL_No])
                                        AddKey(ref RetVal, "Relay" + RL_No.ToString(), "true");

                                EndClass(ref RetVal);
                            }
                        }
                        else // Okuyucu devre dışı.
                        {
                            BeginClass(ref RetVal, "Status");
                            AddKey(ref RetVal, "Disable", "true");
                            EndClass(ref RetVal);
                        }

                        EndClass(ref RetVal);   // } Reader
                    }
                    EndClass(ref RetVal);   // } Readers
                }

                EndClass(ref RetVal);  // } User

                return RetVal;
            }


            public const Int16 NO_CREDIT_USAGE = -1;
            public Protect Protected;

            private UInt64 Kart_Bilgisi;
            private Int64 Sicil_No;
            private UserRelays ARelays;
            private AccessInfo Gecis_Durumu;
            private UInt64 Sifre;
            private Int32 Kontor;
            private Int32 Access_Grubu;
            private UserPGMInfo APGMInfo;
            private UserReaders AReaders;
            private string Kullanici_Adi;
        }
    }
}