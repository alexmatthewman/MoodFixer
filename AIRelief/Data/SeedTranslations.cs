using System.Linq;
using System.Threading.Tasks;
using AIRelief.Models;
using Microsoft.EntityFrameworkCore;

namespace AIRelief.Data
{
    public static class SeedTranslations
    {
        public static async Task Run(AIReliefContext db)
        {
            if (await db.Translations.AnyAsync())
                return;

            db.Translations.AddRange(

                // ?? AI Relief (English) ????????????????????????
                new Translation
                {
                    Key = "SiteDescription",
                    Language = "en",
                    Market = "relief",
                    Value = "A tool to help you adjust and realign"
                },
                new Translation
                {
                    Key = "HeroTitle_Prefix",
                    Language = "en",
                    Market = "relief",
                    Value = "Don't Let AI"
                },
                new Translation
                {
                    Key = "HeroTitle_Accent",
                    Language = "en",
                    Market = "relief",
                    Value = "Dull Your Mind"
                },
                new Translation
                {
                    Key = "HeroSubtitle",
                    Language = "en",
                    Market = "relief",
                    Value = "Reclaim your critical thinking skills with the training platform that strengthens what AI dependency weakens."
                },
                new Translation
                {
                    Key = "CognitiveFitnessProgramTitle",
                    Language = "en",
                    Market = "relief",
                    Value = "Your Cognitive Fitness Program"
                },
                new Translation
                {
                    Key = "CognitiveFitnessProgramSubtitle",
                    Language = "en",
                    Market = "relief",
                    Value = "Structured exercises designed to rebuild and strengthen the critical reasoning skills that AI dependency has weakened."
                },
                new Translation
                {
                    Key = "StartFreeTrial",
                    Language = "en",
                    Market = "relief",
                    Value = "Start Free Trial"
                },
                new Translation
                {
                    Key = "ReadyToReclaim",
                    Language = "en",
                    Market = "relief",
                    Value = "Ready to Reclaim Your Cognitive Edge?"
                },
                new Translation
                {
                    Key = "JoinDevelopers",
                    Language = "en",
                    Market = "relief",
                    Value = "Join thousands of developers who have strengthened their critical thinking skills."
                },
                new Translation
                {
                    Key = "Testimonial1_Quote",
                    Language = "en",
                    Market = "relief",
                    Value = "After 6 months with AI Relief, I can debug complex issues 40% faster and feel confident tackling problems without AI assistance."
                },
                new Translation
                {
                    Key = "Testimonial1_Name",
                    Language = "en",
                    Market = "relief",
                    Value = "Sarah Chen"
                },
                new Translation
                {
                    Key = "Testimonial1_Role",
                    Language = "en",
                    Market = "relief",
                    Value = "Senior Developer, TechCorp"
                },
                new Translation
                {
                    Key = "Testimonial2_Quote",
                    Language = "en",
                    Market = "relief",
                    Value = "The critical thinking exercises helped me realize how much I'd been relying on AI. Now I'm a better problem solver than ever."
                },
                new Translation
                {
                    Key = "Testimonial2_Name",
                    Language = "en",
                    Market = "relief",
                    Value = "Marcus Rodriguez"
                },
                new Translation
                {
                    Key = "Testimonial2_Role",
                    Language = "en",
                    Market = "relief",
                    Value = "Lead Engineer, StartupXYZ"
                },
                new Translation
                {
                    Key = "Testimonial3_Quote",
                    Language = "en",
                    Market = "relief",
                    Value = "AI Relief should be mandatory for any team using AI tools. It's like going to the gym for your brain."
                },
                new Translation
                {
                    Key = "Testimonial3_Name",
                    Language = "en",
                    Market = "relief",
                    Value = "David Kim"
                },
                new Translation
                {
                    Key = "Testimonial3_Role",
                    Language = "en",
                    Market = "relief",
                    Value = "Engineering Manager, BigTech Inc"
                },

                // ?? AICAG (English) ????????????????????????????
                new Translation
                {
                    Key = "SiteDescription",
                    Language = "en",
                    Market = "aicag",
                    Value = "Combat cognitive atrophy with AI-driven brain training"
                },
                new Translation
                {
                    Key = "HeroTitle_Prefix",
                    Language = "en",
                    Market = "aicag",
                    Value = "Train Your Mind."
                },
                new Translation
                {
                    Key = "HeroTitle_Accent",
                    Language = "en",
                    Market = "aicag",
                    Value = "Strengthen Your Future"
                },
                new Translation
                {
                    Key = "HeroSubtitle",
                    Language = "en",
                    Market = "aicag",
                    Value = "Combat cognitive atrophy with AI-driven brain training designed to keep your mind sharp, focused, and resilient."
                },
                new Translation
                {
                    Key = "CognitiveFitnessProgramTitle",
                    Language = "en",
                    Market = "aicag",
                    Value = "Your Cognitive Fitness Programme"
                },
                new Translation
                {
                    Key = "CognitiveFitnessProgramSubtitle",
                    Language = "en",
                    Market = "aicag",
                    Value = "A structured programme of exercises designed to keep your cognitive abilities sharp and resilient."
                },
                new Translation
                {
                    Key = "StartFreeTrial",
                    Language = "en",
                    Market = "aicag",
                    Value = "Start Brain Training"
                },
                new Translation
                {
                    Key = "ReadyToReclaim",
                    Language = "en",
                    Market = "aicag",
                    Value = "Ready to Sharpen Your Mind?"
                },
                new Translation
                {
                    Key = "JoinDevelopers",
                    Language = "en",
                    Market = "aicag",
                    Value = "Join thousands of professionals who have strengthened their cognitive fitness."
                },
                new Translation
                {
                    Key = "Testimonial1_Quote",
                    Language = "en",
                    Market = "aicag",
                    Value = "The brain training exercises have noticeably improved my focus and problem-solving at work."
                },
                new Translation
                {
                    Key = "Testimonial1_Name",
                    Language = "en",
                    Market = "aicag",
                    Value = "Sarah Chen"
                },
                new Translation
                {
                    Key = "Testimonial1_Role",
                    Language = "en",
                    Market = "aicag",
                    Value = "Senior Analyst, TechCorp"
                },
                new Translation
                {
                    Key = "Testimonial2_Quote",
                    Language = "en",
                    Market = "aicag",
                    Value = "I didn't realise how much my critical thinking had dulled until I started these exercises. Highly recommended."
                },
                new Translation
                {
                    Key = "Testimonial2_Name",
                    Language = "en",
                    Market = "aicag",
                    Value = "Marcus Rodriguez"
                },
                new Translation
                {
                    Key = "Testimonial2_Role",
                    Language = "en",
                    Market = "aicag",
                    Value = "Lead Engineer, StartupXYZ"
                },
                new Translation
                {
                    Key = "Testimonial3_Quote",
                    Language = "en",
                    Market = "aicag",
                    Value = "AICAG should be part of every team's cognitive wellness toolkit. It's like a gym for your brain."
                },
                new Translation
                {
                    Key = "Testimonial3_Name",
                    Language = "en",
                    Market = "aicag",
                    Value = "David Kim"
                },
                new Translation
                {
                    Key = "Testimonial3_Role",
                    Language = "en",
                    Market = "aicag",
                    Value = "Engineering Manager, BigTech Inc"
                },

                // ?? AI Descanso (Spanish) ??????????????????????
                new Translation
                {
                    Key = "SiteDescription",
                    Language = "es",
                    Market = "descanso",
                    Value = "Una herramienta para ayudarte a ajustar y realinear"
                },
                new Translation
                {
                    Key = "HeroTitle_Prefix",
                    Language = "es",
                    Market = "descanso",
                    Value = "Tu Camino Hacia"
                },
                new Translation
                {
                    Key = "HeroTitle_Accent",
                    Language = "es",
                    Market = "descanso",
                    Value = "el Bienestar Mental"
                },
                new Translation
                {
                    Key = "HeroSubtitle",
                    Language = "es",
                    Market = "descanso",
                    Value = "Apoyo impulsado por IA para ayudarte a encontrar alivio, desarrollar resiliencia y mantener el equilibrio emocional."
                },
                new Translation
                {
                    Key = "CognitiveFitnessProgramTitle",
                    Language = "es",
                    Market = "descanso",
                    Value = "Bienestar Terapéutico, Reinventado"
                },
                new Translation
                {
                    Key = "CognitiveFitnessProgramSubtitle",
                    Language = "es",
                    Market = "descanso",
                    Value = "Ejercicios estructurados diseñados para reconstruir y fortalecer las habilidades de razonamiento crítico."
                },
                new Translation
                {
                    Key = "StartFreeTrial",
                    Language = "es",
                    Market = "descanso",
                    Value = "Comienza Tu Camino"
                },
                new Translation
                {
                    Key = "ReadyToReclaim",
                    Language = "es",
                    Market = "descanso",
                    Value = "¿Listo para Recuperar tu Ventaja Cognitiva?"
                },
                new Translation
                {
                    Key = "JoinDevelopers",
                    Language = "es",
                    Market = "descanso",
                    Value = "Únete a miles de profesionales que han fortalecido sus habilidades de pensamiento crítico."
                },
                new Translation
                {
                    Key = "Testimonial1_Quote",
                    Language = "es",
                    Market = "descanso",
                    Value = "Después de 6 meses, puedo resolver problemas complejos un 40% más rápido y con más confianza."
                },
                new Translation
                {
                    Key = "Testimonial1_Name",
                    Language = "es",
                    Market = "descanso",
                    Value = "Sara Chen"
                },
                new Translation
                {
                    Key = "Testimonial1_Role",
                    Language = "es",
                    Market = "descanso",
                    Value = "Desarrolladora Senior, TechCorp"
                },
                new Translation
                {
                    Key = "Testimonial2_Quote",
                    Language = "es",
                    Market = "descanso",
                    Value = "Los ejercicios de pensamiento crítico me ayudaron a darme cuenta de cuánto dependía de la IA."
                },
                new Translation
                {
                    Key = "Testimonial2_Name",
                    Language = "es",
                    Market = "descanso",
                    Value = "Marco Rodríguez"
                },
                new Translation
                {
                    Key = "Testimonial2_Role",
                    Language = "es",
                    Market = "descanso",
                    Value = "Ingeniero Principal, StartupXYZ"
                },
                new Translation
                {
                    Key = "Testimonial3_Quote",
                    Language = "es",
                    Market = "descanso",
                    Value = "AI Descanso debería ser obligatorio para cualquier equipo que use herramientas de IA."
                },
                new Translation
                {
                    Key = "Testimonial3_Name",
                    Language = "es",
                    Market = "descanso",
                    Value = "David Kim"
                },
                new Translation
                {
                    Key = "Testimonial3_Role",
                    Language = "es",
                    Market = "descanso",
                    Value = "Gerente de Ingeniería, BigTech Inc"
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
