using System.Threading.Tasks;
using AIRelief.Models;
using Microsoft.EntityFrameworkCore;

namespace AIRelief.Data
{
    public static class SeedEmailTemplates
    {
        public static async Task Run(AIReliefContext db)
        {
            if (await db.EmailTemplates.AnyAsync())
                return;

            db.EmailTemplates.AddRange(

                // ?? AI Relief (English) ??????????????????????????
                new EmailTemplate
                {
                    TemplateKey = "FeedbackAcknowledgment",
                    Language = "en",
                    Market = "relief",
                    Subject = "Thank you for contacting {SiteName}",
                    Body = "Dear {Name},\n\n"
                         + "Thank you for reaching out to {SiteName}. We have received your {FeedbackType} and a member of our team will review it shortly.\n\n"
                         + "We aim to respond to all correspondence in a timely manner and appreciate your patience.\n\n"
                         + "Kind regards,\n"
                         + "The {SiteName} Team"
                },

                // ?? AICAG (English) ??????????????????????????????
                new EmailTemplate
                {
                    TemplateKey = "FeedbackAcknowledgment",
                    Language = "en",
                    Market = "aicag",
                    Subject = "Thank you for contacting {SiteName}",
                    Body = "Dear {Name},\n\n"
                         + "Thank you for reaching out to {SiteName}. We have received your {FeedbackType} and a member of our team will review it shortly.\n\n"
                         + "We aim to respond to all correspondence in a timely manner and appreciate your patience.\n\n"
                         + "Kind regards,\n"
                         + "The {SiteName} Team"
                },

                // ?? AI Descanso (Spanish) ????????????????????????
                new EmailTemplate
                {
                    TemplateKey = "FeedbackAcknowledgment",
                    Language = "es",
                    Market = "descanso",
                    Subject = "Gracias por contactar a {SiteName}",
                    Body = "Estimado/a {Name},\n\n"
                         + "Gracias por comunicarse con {SiteName}. Hemos recibido su {FeedbackType} y un miembro de nuestro equipo lo revisará en breve.\n\n"
                         + "Nuestro objetivo es responder a toda la correspondencia de manera oportuna y agradecemos su paciencia.\n\n"
                         + "Saludos cordiales,\n"
                         + "El equipo de {SiteName}"
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
