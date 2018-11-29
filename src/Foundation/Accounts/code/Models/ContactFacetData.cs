namespace Sitecore.HabitatHome.Foundation.Accounts.Models
{
    public class ContactFacetData 
    {
        public string EmailAddress { get; set; }

        public string EmailKey { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string PhoneKey { get; set; }

        public string AvatarUrl { get; set; }

        public string AvatarMimeType { get; set; }

        public string Gender { get; set; }

        public string Birthday { get; set; }

        public string Language { get; set; }        
        
        public string SportName { get; set; }

        public string SportType { get; set; }

        public string this[string key]
        {
            get
            {
                switch (key)
                {
                    case "EmailAddress": return this.EmailAddress;
                    case "EmailKey": return this.EmailKey;
                    case "FirstName": return this.FirstName;
                    case "MiddleName": return this.MiddleName;
                    case "LastName": return this.LastName;
                    case "PhoneNumber": return this.PhoneNumber;
                    case "PhoneKey": return this.PhoneKey;
                    case "AvatarUrl": return this.AvatarUrl;
                    case "AvatarMimeType": return this.AvatarMimeType;
                    case "Gender": return this.Gender;
                    case "Birthday": return this.Birthday;
                    case "Language": return this.Language;
                    case "SportType": return this.SportType;
                    case "SportName": return this.SportName;
                    default: return string.Empty;
                }
            }
            set
            {
                switch (key)
                {
                    case "Email": this.EmailAddress = value;
                        break;
                    case "EmailKey": this.EmailKey = value;
                        break;
                    case "FirstName": this.FirstName = value;
                        break;
                    case "MiddleName": this.MiddleName = value;
                        break;
                    case "LastName": this.LastName = value;
                        break;
                    case "PhoneNumber": this.PhoneNumber = value;
                        break;
                    case "PhoneKey": this.PhoneKey = value;
                        break;
                    case "AvatarUrl": this.AvatarUrl = value;
                        break;
                    case "AvatarMimeType": this.AvatarMimeType = value;
                        break;
                    case "Gender": this.Gender = value;
                        break;
                    case "Birthday": this.Birthday = value;
                        break;
                    case "Language": this.Language = value;
                        break;
                    case "SportType": this.SportType = value;
                        break;
                    case "SportName":this.SportName = value;
                        break;
                    
                    default: return;
                }
            }
        }
    }
}