using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence;

/// <summary>
/// Seeds essential lookup data on first startup.
/// All operations are idempotent — safe to run on every startup.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken ct = default)
    {
        // ── Tenant Statuses ───────────────────────────────────────────────────────
        if (!await context.TenantStatuses.AnyAsync(ct))
        {
            var tenantStatuses = new List<TenantStatus>
            {
                TenantStatus.Create(TenantStatusCodes.PendingEmailVerification, "Pending Email Verification",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Verificación de correo pendiente"),
                        BaseTranslatedProperties.CreateWithName("Pending Email Verification"))),

                TenantStatus.Create(TenantStatusCodes.PendingSubscription, "Pending Subscription",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Suscripción pendiente"),
                        BaseTranslatedProperties.CreateWithName("Pending Subscription"))),

                TenantStatus.Create(TenantStatusCodes.Active, "Active",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Activo"),
                        BaseTranslatedProperties.CreateWithName("Active"))),

                TenantStatus.Create(TenantStatusCodes.Suspended, "Suspended",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Suspendido"),
                        BaseTranslatedProperties.CreateWithName("Suspended"))),

                TenantStatus.Create(TenantStatusCodes.Cancelled, "Cancelled",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Cancelado"),
                        BaseTranslatedProperties.CreateWithName("Cancelled"))),
            };

            await context.TenantStatuses.AddRangeAsync(tenantStatuses, ct);
            await context.SaveChangesAsync(ct);
        }

        // ── Subscription Statuses ─────────────────────────────────────────────────
        if (!await context.SubscriptionStatuses.AnyAsync(ct))
        {
            var subscriptionStatuses = new List<SubscriptionStatus>
            {
                SubscriptionStatus.Create(SubscriptionStatusCodes.Trial, "Trial",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Prueba"),
                        BaseTranslatedProperties.CreateWithName("Trial"))),

                SubscriptionStatus.Create(SubscriptionStatusCodes.Active, "Active",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Activo"),
                        BaseTranslatedProperties.CreateWithName("Active"))),

                SubscriptionStatus.Create(SubscriptionStatusCodes.PastDue, "Past Due",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Vencido"),
                        BaseTranslatedProperties.CreateWithName("Past Due"))),

                SubscriptionStatus.Create(SubscriptionStatusCodes.Suspended, "Suspended",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Suspendido"),
                        BaseTranslatedProperties.CreateWithName("Suspended"))),

                SubscriptionStatus.Create(SubscriptionStatusCodes.Cancelled, "Cancelled",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Cancelado"),
                        BaseTranslatedProperties.CreateWithName("Cancelled"))),

                SubscriptionStatus.Create(SubscriptionStatusCodes.Expired, "Expired",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Expirado"),
                        BaseTranslatedProperties.CreateWithName("Expired"))),
            };

            await context.SubscriptionStatuses.AddRangeAsync(subscriptionStatuses, ct);
            await context.SaveChangesAsync(ct);
        }

        // ── Subscription Statuses (incremental) ──────────────────────────────────
        if (!await context.SubscriptionStatuses.AnyAsync(s => s.Code == SubscriptionStatusCodes.ProcessingPayment, ct))
        {
            context.SubscriptionStatuses.Add(SubscriptionStatus.Create(SubscriptionStatusCodes.ProcessingPayment, "Processing Payment",
                TranslatedString.Create(
                    BaseTranslatedProperties.CreateWithName("Procesando pago"),
                    BaseTranslatedProperties.CreateWithName("Processing Payment"))));

            await context.SaveChangesAsync(ct);
        }

        // ── Languages ─────────────────────────────────────────────────────────────
        if (!await context.Languages.AnyAsync(ct))
        {
            var languages = new List<Language>
            {
                Language.Create("es", "Spanish",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Español"),
                        BaseTranslatedProperties.CreateWithName("Spanish")),
                    isDefault: true),

                Language.Create("en", "English",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Inglés"),
                        BaseTranslatedProperties.CreateWithName("English"))),
            };

            await context.Languages.AddRangeAsync(languages, ct);
            await context.SaveChangesAsync(ct);
        }

        // ── Genders ───────────────────────────────────────────────────────────────
        if (!await context.Genders.AnyAsync(ct))
        {
            var genders = new List<Gender>
            {
                Gender.Create("MALE", "Male",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Masculino"),
                        BaseTranslatedProperties.CreateWithName("Male"))),

                Gender.Create("FEMALE", "Female",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Femenino"),
                        BaseTranslatedProperties.CreateWithName("Female"))),

                Gender.Create("OTHER", "Other",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Otro"),
                        BaseTranslatedProperties.CreateWithName("Other"))),
            };

            await context.Genders.AddRangeAsync(genders, ct);
            await context.SaveChangesAsync(ct);
        }

        // ── Billing Cycles ────────────────────────────────────────────────────────
        if (!await context.BillingCycles.AnyAsync(ct))
        {
            var billingCycles = new List<BillingCycle>
            {
                BillingCycle.Create("MONTHLY", "Monthly", "Billed every month",
                    TranslatedString.Create(
                        BaseTranslatedProperties.Create("Mensual", "Facturado cada mes"),
                        BaseTranslatedProperties.Create("Monthly", "Billed every month")),
                    months: 1),

                BillingCycle.Create("QUARTERLY", "Quarterly", "Billed every 3 months",
                    TranslatedString.Create(
                        BaseTranslatedProperties.Create("Trimestral", "Facturado cada 3 meses"),
                        BaseTranslatedProperties.Create("Quarterly", "Billed every 3 months")),
                    months: 3),

                BillingCycle.Create("ANNUAL", "Annual", "Billed once per year",
                    TranslatedString.Create(
                        BaseTranslatedProperties.Create("Anual", "Facturado una vez al año"),
                        BaseTranslatedProperties.Create("Annual", "Billed once per year")),
                    months: 12),
            };

            await context.BillingCycles.AddRangeAsync(billingCycles, ct);
            await context.SaveChangesAsync(ct);
        }

        // ── Option Actions ────────────────────────────────────────────────────────
        if (!await context.OptionActions.AnyAsync(ct))
        {
            var optionActions = new List<OptionAction>
            {
                OptionAction.Create("VIEW", "View",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Ver"),
                        BaseTranslatedProperties.CreateWithName("View"))),

                OptionAction.Create("CREATE", "Create",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Crear"),
                        BaseTranslatedProperties.CreateWithName("Create"))),

                OptionAction.Create("EDIT", "Edit",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Editar"),
                        BaseTranslatedProperties.CreateWithName("Edit"))),

                OptionAction.Create("DELETE", "Delete",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Eliminar"),
                        BaseTranslatedProperties.CreateWithName("Delete"))),

                OptionAction.Create("EXPORT", "Export",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Exportar"),
                        BaseTranslatedProperties.CreateWithName("Export"))),

                OptionAction.Create("PRINT", "Print",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Imprimir"),
                        BaseTranslatedProperties.CreateWithName("Print"))),
            };

            await context.OptionActions.AddRangeAsync(optionActions, ct);
            await context.SaveChangesAsync(ct);
        }

        // ── Countries ─────────────────────────────────────────────────────────────
        if (!await context.Countries.AnyAsync(ct))
        {
            var countries = new List<Country>
            {
                Country.Create("DO", "Dominican Republic", "DOM",
                    TranslatedString.Create(
                        BaseTranslatedProperties.CreateWithName("Republica Dominicana"),
                        BaseTranslatedProperties.CreateWithName("Dominican Republic")),
                    "+1"),
            };

            await context.Countries.AddRangeAsync(countries, ct);
            await context.SaveChangesAsync(ct);
        }

        // ── Provinces ────────────────────────────────────────────────────────────
        if (!await context.Provinces.AnyAsync(ct))
        {
            var country = await context.Countries.FirstAsync(ct);

            var provinces = new List<Province>
            {
                Province.Create("1",  "DISTRITO NACIONAL",        country.Id),
                Province.Create("2",  "LA ALTAGRACIA",            country.Id),
                Province.Create("3",  "AZUA",                     country.Id),
                Province.Create("4",  "BAHORUCO",                 country.Id),
                Province.Create("5",  "BARAHONA",                 country.Id),
                Province.Create("6",  "DAJABON",                  country.Id),
                Province.Create("7",  "DUARTE",                   country.Id),
                Province.Create("8",  "EL SEYBO",                 country.Id),
                Province.Create("9",  "ELIAS PIÑA",               country.Id),
                Province.Create("10", "ESPAILLAT",                country.Id),
                Province.Create("11", "HATO MAYOR",               country.Id),
                Province.Create("12", "INDEPENDENCIA",            country.Id),
                Province.Create("13", "LA ROMANA",                country.Id),
                Province.Create("14", "LA VEGA",                  country.Id),
                Province.Create("15", "MARIA TRINIDAD SANCHEZ",   country.Id),
                Province.Create("16", "MONSEÑOR NOUEL",           country.Id),
                Province.Create("17", "MONTECRISTI",              country.Id),
                Province.Create("18", "MONTE PLATA",              country.Id),
                Province.Create("19", "PEDERNALES",               country.Id),
                Province.Create("20", "PERAVIA",                  country.Id),
                Province.Create("21", "PUERTO PLATA",             country.Id),
                Province.Create("22", "HERMANAS MIRABAL",         country.Id),
                Province.Create("23", "SAMANA",                   country.Id),
                Province.Create("24", "SAN CRISTOBAL",            country.Id),
                Province.Create("25", "SAN JUAN",                 country.Id),
                Province.Create("26", "SAN PEDRO DE MACORIS",     country.Id),
                Province.Create("27", "SANCHEZ RAMIREZ",          country.Id),
                Province.Create("28", "SANTIAGO",                 country.Id),
                Province.Create("29", "SANTIAGO RODRIGUEZ",       country.Id),
                Province.Create("30", "VALVERDE",                 country.Id),
                Province.Create("31", "SAN JOSE DE OCOA",         country.Id),
                Province.Create("32", "SANTO DOMINGO",            country.Id),
            };

            await context.Provinces.AddRangeAsync(provinces, ct);
            await context.SaveChangesAsync(ct);
        }

        // ── Municipalities ────────────────────────────────────────────────────────
        if (!await context.Municipalities.AnyAsync(ct))
        {
            var p = await context.Provinces.OrderBy(x => x.Id).ToListAsync(ct);

            var municipalities = new List<Municipality>
            {
                Municipality.Create("1",   "SANTO DOMINGO DE GUZMAN",                               p[0].Id),
                Municipality.Create("2",   "SALVALEON DE HIGUEY",                                   p[1].Id),
                Municipality.Create("3",   "SAN RAFAEL DEL YUMA",                                   p[1].Id),
                Municipality.Create("4",   "LA OTRA BANDA (D.M.)",                                  p[1].Id),
                Municipality.Create("5",   "LAS LAGUNAS DE NISIBON (D.M.)",                         p[1].Id),
                Municipality.Create("6",   "BOCA DE YUMA (D.M.)",                                   p[1].Id),
                Municipality.Create("7",   "BAYAHÍBE (D.M.)",                                       p[1].Id),
                Municipality.Create("8",   "VERÓN PUNTA CANA (D.M.)",                               p[1].Id),
                Municipality.Create("9",   "AZUA DE COMPOSTELA",                                    p[2].Id),
                Municipality.Create("10",  "PADRE LAS CASAS",                                       p[2].Id),
                Municipality.Create("11",  "LAS CHARCAS (DM)",                                      p[2].Id),
                Municipality.Create("12",  "LAS YAYAS DE VIAJAMAS (DM)",                            p[2].Id),
                Municipality.Create("13",  "TABARA ARRIBA (DM)",                                    p[2].Id),
                Municipality.Create("14",  "GUAYABAL (DM)",                                         p[2].Id),
                Municipality.Create("15",  "ESTEBANIA (DM)",                                        p[2].Id),
                Municipality.Create("16",  "PERALTA",                                               p[2].Id),
                Municipality.Create("17",  "SABANA YEGUA (DM)",                                     p[2].Id),
                Municipality.Create("18",  "PUEBLO VIEJO",                                          p[2].Id),
                Municipality.Create("19",  "PALMAR DE OCOA (D.M.)",                                 p[2].Id),
                Municipality.Create("20",  "LOS TOROS (D.M.)",                                      p[2].Id),
                Municipality.Create("21",  "GANADERO (D.M.)",                                       p[2].Id),
                Municipality.Create("22",  "EL ROSARIO (D.M.)",                                     p[2].Id),
                Municipality.Create("23",  "LAS BARÍAS-LA ESTANCIA (D.M.)",                         p[2].Id),
                Municipality.Create("24",  "LOS JOVILLOS (D.M.)",                                   p[2].Id),
                Municipality.Create("25",  "BARRO ARRIBA (D.M.)",                                   p[2].Id),
                Municipality.Create("26",  "AMIAMA GÓMEZ (D.M.)",                                   p[2].Id),
                Municipality.Create("27",  "TÁBARA ABAJO (D.M.)",                                   p[2].Id),
                Municipality.Create("28",  "PROYECTO 4 (D.M.)",                                     p[2].Id),
                Municipality.Create("29",  "LAS LAGUNAS (D.M.)",                                    p[2].Id),
                Municipality.Create("30",  "LA SIEMBRA (D.M.)",                                     p[2].Id),
                Municipality.Create("31",  "VILLARPANDO (D.M.)",                                    p[2].Id),
                Municipality.Create("32",  "BARRERAS (D.M.)",                                       p[2].Id),
                Municipality.Create("33",  "PUERTO VIEJO (D.M.)",                                   p[2].Id),
                Municipality.Create("34",  "MONTE BONITO (D.M.)",                                   p[2].Id),
                Municipality.Create("35",  "DOÑA EMMA BALAGUER VIUDA VALLEJO (D.M.)",               p[2].Id),
                Municipality.Create("36",  "CLAVELLINA (D.M.)",                                     p[2].Id),
                Municipality.Create("37",  "LOS FRÍOS (D.M.)",                                      p[2].Id),
                Municipality.Create("38",  "LAS LOMAS (D.M.)",                                      p[2].Id),
                Municipality.Create("39",  "HATO NUEVO CORTÉS (D.M.)",                              p[2].Id),
                Municipality.Create("40",  "PROYECTO 2-C (D.M.)",                                   p[2].Id),
                Municipality.Create("41",  "NEYBA",                                                 p[3].Id),
                Municipality.Create("42",  "GALVAN",                                                p[3].Id),
                Municipality.Create("43",  "LOS RIOS (DM)",                                         p[3].Id),
                Municipality.Create("44",  "TAMAYO",                                                p[3].Id),
                Municipality.Create("45",  "VILLA JARAGUA",                                         p[3].Id),
                Municipality.Create("46",  "UVILLA (DM)",                                           p[3].Id),
                Municipality.Create("47",  "EL PALMAR (D.M.)",                                      p[3].Id),
                Municipality.Create("48",  "LAS CLAVELLINAS (D.M.)",                                p[3].Id),
                Municipality.Create("49",  "SANTANA (D.M.)",                                        p[3].Id),
                Municipality.Create("50",  "MONSERRAT (D.M.)",                                      p[3].Id),
                Municipality.Create("51",  "CABEZA DE TORO (D.M.)",                                 p[3].Id),
                Municipality.Create("52",  "MENA (D.M.)",                                           p[3].Id),
                Municipality.Create("53",  "SANTA BÁRBARA EL 6 (D.M.)",                             p[3].Id),
                Municipality.Create("54",  "EL SALADO (D.M.)",                                      p[3].Id),
                Municipality.Create("55",  "SANTA CRUZ DE BARAHONA",                                p[4].Id),
                Municipality.Create("56",  "CABRAL",                                                p[4].Id),
                Municipality.Create("57",  "ENRIQUILLO",                                            p[4].Id),
                Municipality.Create("58",  "PARAISO",                                               p[4].Id),
                Municipality.Create("59",  "VICENTE NOBLE",                                         p[4].Id),
                Municipality.Create("60",  "EL PEÑON (DM)",                                         p[4].Id),
                Municipality.Create("61",  "FUNDACION (DM)",                                        p[4].Id),
                Municipality.Create("62",  "LAS SALINAS (DM)",                                      p[4].Id),
                Municipality.Create("63",  "POLO (DM)",                                             p[4].Id),
                Municipality.Create("64",  "LA CIÉNAGA",                                            p[4].Id),
                Municipality.Create("65",  "CANOA (D.M.)",                                          p[4].Id),
                Municipality.Create("66",  "JAQUIMEYES",                                            p[4].Id),
                Municipality.Create("67",  "EL CACHÓN (D.M)",                                       p[4].Id),
                Municipality.Create("68",  "PESCADERÍA (D.M.)",                                     p[4].Id),
                Municipality.Create("69",  "LOS PATOS (D.M.)",                                      p[4].Id),
                Municipality.Create("70",  "QUITA CORAZA (D.M.)",                                   p[4].Id),
                Municipality.Create("71",  "FONDO NEGRO (D.M.)",                                    p[4].Id),
                Municipality.Create("72",  "ARROYO DULCE (D.M.)",                                   p[4].Id),
                Municipality.Create("73",  "PALO ALTO (D.M.)",                                      p[4].Id),
                Municipality.Create("74",  "BAHORUCO (D.M.)",                                       p[4].Id),
                Municipality.Create("75",  "LA GUÁZARA (D.M)",                                      p[4].Id),
                Municipality.Create("76",  "VILLA CENTRAL (D.M)",                                   p[4].Id),
                Municipality.Create("77",  "RESTAURACION",                                          p[5].Id),
                Municipality.Create("78",  "DAJABON",                                               p[5].Id),
                Municipality.Create("79",  "LOMA DE CABRERA",                                       p[5].Id),
                Municipality.Create("80",  "PARTIDO",                                               p[5].Id),
                Municipality.Create("81",  "EL PINO",                                               p[5].Id),
                Municipality.Create("82",  "MANUEL BUENO (D.M.)",                                   p[5].Id),
                Municipality.Create("83",  "CAPOTILLO (D.M.)",                                      p[5].Id),
                Municipality.Create("84",  "CAÑONGO (D.M.)",                                        p[5].Id),
                Municipality.Create("85",  "SANTIAGO DE LA CRUZ (D.M.)",                            p[5].Id),
                Municipality.Create("86",  "SAN FRANCISCO DE MACORIS",                              p[6].Id),
                Municipality.Create("87",  "PIMENTEL",                                              p[6].Id),
                Municipality.Create("88",  "VILLA RIVA",                                            p[6].Id),
                Municipality.Create("89",  "CASTILLO",                                              p[6].Id),
                Municipality.Create("90",  "HOSTOS (DM)",                                           p[6].Id),
                Municipality.Create("91",  "ARENOSO",                                               p[6].Id),
                Municipality.Create("92",  "LAS GUÁRANAS",                                          p[6].Id),
                Municipality.Create("93",  "AGUA SANTA DEL YUNA (D.M.)",                            p[6].Id),
                Municipality.Create("94",  "CRISTO REY DE GUARAGUAO (D.M.)",                        p[6].Id),
                Municipality.Create("95",  "LA PEÑA (D.M.)",                                        p[6].Id),
                Municipality.Create("96",  "CENOVÍ (D.M.)",                                         p[6].Id),
                Municipality.Create("97",  "LAS COLES (D.M.)",                                      p[6].Id),
                Municipality.Create("98",  "LAS TARANAS (D.M.)",                                    p[6].Id),
                Municipality.Create("99",  "SABANA GRANDE (D.M.)",                                  p[6].Id),
                Municipality.Create("100", "EL AGUACATE (D.M.)",                                    p[6].Id),
                Municipality.Create("101", "BARRAQUITO (D. M.)",                                    p[6].Id),
                Municipality.Create("102", "JAYA (D.M.)",                                           p[6].Id),
                Municipality.Create("103", "PRESIDENTE DON ANTONIO GUZMÁN FERNÁNDEZ D.M.",          p[6].Id),
                Municipality.Create("104", "SANTA CRUZ DEL SEIBO",                                  p[7].Id),
                Municipality.Create("105", "MICHES",                                                p[7].Id),
                Municipality.Create("106", "PEDRO SÁNCHEZ (D.M.)",                                  p[7].Id),
                Municipality.Create("107", "EL CEDRO (D.M.)",                                       p[7].Id),
                Municipality.Create("108", "LA GINA (D.M.)",                                        p[7].Id),
                Municipality.Create("109", "SAN FRANCISCO-VICENTILLO D.M.",                         p[7].Id),
                Municipality.Create("110", "SANTA LUCÍA D.M.",                                      p[7].Id),
                Municipality.Create("111", "BANICA",                                                p[8].Id),
                Municipality.Create("112", "COMENDADOR DE LARES (ELIAS PIÑA)",                      p[8].Id),
                Municipality.Create("113", "EL LLANO",                                              p[8].Id),
                Municipality.Create("114", "HONDO VALLE",                                           p[8].Id),
                Municipality.Create("115", "PEDRO SANTANA",                                         p[8].Id),
                Municipality.Create("116", "JUAN SANTIAGO",                                         p[8].Id),
                Municipality.Create("117", "RÍO LIMPIO (D.M.)",                                     p[8].Id),
                Municipality.Create("118", "SABANA LARGA (D.M.)",                                   p[8].Id),
                Municipality.Create("119", "GUANITO (D.M.)",                                        p[8].Id),
                Municipality.Create("120", "SABANA CRUZ (D.M.)",                                    p[8].Id),
                Municipality.Create("121", "GUAYABO (D.M.)",                                        p[8].Id),
                Municipality.Create("122", "SABANA HIGÜERO (D.M.)",                                 p[8].Id),
                Municipality.Create("123", "RANCHO DE LA GUARDIA (D.M.)",                           p[8].Id),
                Municipality.Create("124", "MOCA",                                                  p[9].Id),
                Municipality.Create("125", "CAYETANO GERMOSEN",                                     p[9].Id),
                Municipality.Create("126", "GASPAR HERNANDEZ",                                      p[9].Id),
                Municipality.Create("127", "JAMAO AL NORTE (DM)",                                   p[9].Id),
                Municipality.Create("128", "JOSE CONTRERAS (DM)",                                   p[9].Id),
                Municipality.Create("129", "SAN VÍCTOR (D.M.)",                                     p[9].Id),
                Municipality.Create("130", "JOBA ARRIBA (D.M.)",                                    p[9].Id),
                Municipality.Create("131", "JUAN LÓPEZ (D.M.)",                                     p[9].Id),
                Municipality.Create("132", "LAS LAGUNAS (D.M.)",                                    p[9].Id),
                Municipality.Create("133", "VERAGUA (D.M.)",                                        p[9].Id),
                Municipality.Create("134", "EL HIGÜERITO (D.M.)",                                   p[9].Id),
                Municipality.Create("135", "MONTE DE LA JAGUA (D.M)",                               p[9].Id),
                Municipality.Create("136", "LA ORTEGA (D.M.)",                                      p[9].Id),
                Municipality.Create("137", "CANCA LA REYNA (D.M)",                                  p[9].Id),
                Municipality.Create("138", "VILLA MAGANTE (D.M.)",                                  p[9].Id),
                Municipality.Create("139", "HATO MAYOR DEL REY",                                    p[10].Id),
                Municipality.Create("140", "SABANA DE LA MAR",                                      p[10].Id),
                Municipality.Create("141", "EL VALLE",                                              p[10].Id),
                Municipality.Create("142", "ELUPINA CORDERO DE LAS CAÑITAS (D. M.)",                p[10].Id),
                Municipality.Create("143", "YERBA BUENA (D.M.)",                                    p[10].Id),
                Municipality.Create("144", "MATA PALACIO (D.M.)",                                   p[10].Id),
                Municipality.Create("145", "GUAYABO DULCE (D.M.)",                                  p[10].Id),
                Municipality.Create("146", "DUVERGE",                                               p[11].Id),
                Municipality.Create("147", "LA DESCUBIERTA",                                        p[11].Id),
                Municipality.Create("148", "JIMANI",                                                p[11].Id),
                Municipality.Create("149", "POSTRER RIO",                                           p[11].Id),
                Municipality.Create("150", "MELLA (DM)",                                            p[11].Id),
                Municipality.Create("151", "CRISTOBAL (DM)",                                        p[11].Id),
                Municipality.Create("152", "GUAYABAL (D.M.)",                                       p[11].Id),
                Municipality.Create("153", "EL LIMÓN (D.M)",                                        p[11].Id),
                Municipality.Create("154", "LA COLONIA (D.M.)",                                     p[11].Id),
                Municipality.Create("155", "BOCA DE CACHÓN (D.M)",                                  p[11].Id),
                Municipality.Create("156", "VENGAN A VER (D.M.)",                                   p[11].Id),
                Municipality.Create("157", "BATEY 8 (D.M.)",                                        p[11].Id),
                Municipality.Create("158", "LA ROMANA",                                             p[12].Id),
                Municipality.Create("159", "GUAYMATE",                                              p[12].Id),
                Municipality.Create("160", "VILLA HERMOSA",                                         p[12].Id),
                Municipality.Create("161", "CUMAYASA (D.M.)",                                       p[12].Id),
                Municipality.Create("162", "CALETA (D.M.)",                                         p[12].Id),
                Municipality.Create("163", "CONCEPCION DE LA VEGA",                                 p[13].Id),
                Municipality.Create("164", "JARABACOA",                                             p[13].Id),
                Municipality.Create("165", "CONSTANZA",                                             p[13].Id),
                Municipality.Create("166", "JIMA ABAJO",                                            p[13].Id),
                Municipality.Create("167", "RÍO VERDE ARRIBA (D.M.)",                               p[13].Id),
                Municipality.Create("168", "RINCÓN (D.M.)",                                         p[13].Id),
                Municipality.Create("169", "TIREO (D.M.)",                                          p[13].Id),
                Municipality.Create("170", "LA SABINA (D.M.)",                                      p[13].Id),
                Municipality.Create("171", "EL RANCHITO (D.M.)",                                    p[13].Id),
                Municipality.Create("172", "BUENA VISTA (D.M.)",                                    p[13].Id),
                Municipality.Create("173", "MANABAO (D.M.)",                                        p[13].Id),
                Municipality.Create("174", "TAVERA (D.M.)",                                         p[13].Id),
                Municipality.Create("175", "NAGUA",                                                 p[14].Id),
                Municipality.Create("176", "CABRERA",                                               p[14].Id),
                Municipality.Create("177", "RIO SAN JUAN",                                          p[14].Id),
                Municipality.Create("178", "EL FACTOR",                                             p[14].Id),
                Municipality.Create("179", "SAN JOSÉ DE MATANZAS (D.M.)",                           p[14].Id),
                Municipality.Create("180", "ARROYO SALADO (D.M.)",                                  p[14].Id),
                Municipality.Create("181", "LA ENTRADA (D.M.)",                                     p[14].Id),
                Municipality.Create("182", "EL POZO D.M.",                                          p[14].Id),
                Municipality.Create("183", "LAS GORDAS (D.M.)",                                     p[14].Id),
                Municipality.Create("184", "ARROYO AL MEDIO (D.M.)",                                p[14].Id),
                Municipality.Create("185", "BONAO",                                                 p[15].Id),
                Municipality.Create("186", "MAIMON",                                                p[15].Id),
                Municipality.Create("187", "PIEDRA BLANCA",                                         p[15].Id),
                Municipality.Create("188", "VILLA DE SONADOR (D.M.)",                               p[15].Id),
                Municipality.Create("189", "SABANA DEL PUERTO (D.M.)",                              p[15].Id),
                Municipality.Create("190", "JUAN ADRIÁN (D.M.)",                                    p[15].Id),
                Municipality.Create("191", "JUMA BEJUCAL (D.M.)",                                   p[15].Id),
                Municipality.Create("192", "ARROYO TORO - MASIPEDRO (D.M.)",                        p[15].Id),
                Municipality.Create("193", "JAYACO (D.M.)",                                         p[15].Id),
                Municipality.Create("194", "LA SALVIA - LOS QUEMADOS (D.M.)",                       p[15].Id),
                Municipality.Create("195", "SAN FERNANDO DE MONTECRISTI",                           p[16].Id),
                Municipality.Create("196", "GUAYUBIN",                                              p[16].Id),
                Municipality.Create("197", "VILLA VASQUEZ",                                         p[16].Id),
                Municipality.Create("198", "PEPILLO SALCEDO",                                       p[16].Id),
                Municipality.Create("199", "CASTAÑUELAS",                                           p[16].Id),
                Municipality.Create("200", "LAS MATAS DE SANTA CRUZ",                               p[16].Id),
                Municipality.Create("201", "VILLA ELISA (D.M.)",                                    p[16].Id),
                Municipality.Create("202", "HATILLO PALMA (D.M.)",                                  p[16].Id),
                Municipality.Create("203", "CANA CHAPETÓN (D.M.)",                                  p[16].Id),
                Municipality.Create("204", "PALO VERDE (D.M.)",                                     p[16].Id),
                Municipality.Create("205", "BAYAGUANA",                                             p[17].Id),
                Municipality.Create("206", "YAMASA",                                                p[17].Id),
                Municipality.Create("207", "MONTE PLATA",                                           p[17].Id),
                Municipality.Create("208", "SABANA GRANDE DE BOYA",                                 p[17].Id),
                Municipality.Create("209", "PERALVILLO",                                            p[17].Id),
                Municipality.Create("210", "DON JUAN (D.M.)",                                       p[17].Id),
                Municipality.Create("211", "LOS BOTADOS (D.M.)",                                    p[17].Id),
                Municipality.Create("212", "GONZALO (D.M.)",                                        p[17].Id),
                Municipality.Create("213", "CHIRINO (D.M.)",                                        p[17].Id),
                Municipality.Create("214", "MAJAGUAL (D.M.)",                                       p[17].Id),
                Municipality.Create("215", "BOYÁ (D.M.)",                                           p[17].Id),
                Municipality.Create("216", "MAMÁ TINGÓ (D. M.)",                                    p[17].Id),
                Municipality.Create("217", "PEDERNALES",                                            p[18].Id),
                Municipality.Create("218", "OVIEDO",                                                p[18].Id),
                Municipality.Create("219", "JUANCHO (D.M.)",                                        p[18].Id),
                Municipality.Create("220", "JOSÉ FRANCISCO PEÑA GÓMEZ (D.M.)",                      p[18].Id),
                Municipality.Create("221", "BANI",                                                  p[19].Id),
                Municipality.Create("222", "NIZAO",                                                 p[19].Id),
                Municipality.Create("223", "MATANZAS (D.M.)",                                       p[19].Id),
                Municipality.Create("224", "VILLA FUNDACIÓN (D.M.)",                                p[19].Id),
                Municipality.Create("225", "SABANA BUEY (D.M.)",                                    p[19].Id),
                Municipality.Create("226", "PIZARRETE (D.M.)",                                      p[19].Id),
                Municipality.Create("227", "SANTANA (D.M.)",                                        p[19].Id),
                Municipality.Create("228", "PAYA (D.M.)",                                           p[19].Id),
                Municipality.Create("229", "VILLA SOMBRERO (D.M.)",                                 p[19].Id),
                Municipality.Create("230", "EL CARRETÓN (D.M.)",                                    p[19].Id),
                Municipality.Create("231", "CATALINA (D.M.)",                                       p[19].Id),
                Municipality.Create("232", "EL LIMÓNAL (D.M.)",                                     p[19].Id),
                Municipality.Create("233", "LAS BARÍAS (D.M.)",                                     p[19].Id),
                Municipality.Create("234", "SAN FELIPE DE PUERTO PLATA",                            p[20].Id),
                Municipality.Create("235", "IMBERT",                                                p[20].Id),
                Municipality.Create("236", "ALTAMIRA",                                              p[20].Id),
                Municipality.Create("237", "LUPERON",                                               p[20].Id),
                Municipality.Create("238", "SOSUA",                                                 p[20].Id),
                Municipality.Create("239", "LOS HIDALGOS",                                          p[20].Id),
                Municipality.Create("240", "GUANANICO",                                             p[20].Id),
                Municipality.Create("241", "VILLA ISABELA",                                         p[20].Id),
                Municipality.Create("242", "VILLA MONTELLANO",                                      p[20].Id),
                Municipality.Create("243", "ESTERO HONDO (D.M.)",                                   p[20].Id),
                Municipality.Create("244", "LA ISABELA (D.M.)",                                     p[20].Id),
                Municipality.Create("245", "BELLOSO (D.M.)",                                        p[20].Id),
                Municipality.Create("246", "CABARETE (D.M.)",                                       p[20].Id),
                Municipality.Create("247", "SABANETA DE YÁSICA (D.M.)",                             p[20].Id),
                Municipality.Create("248", "LA JAIBA (D.M.)",                                       p[20].Id),
                Municipality.Create("249", "NAVAS (D.M.)",                                          p[20].Id),
                Municipality.Create("250", "YÁSICA ARRIBA (D.M.)",                                  p[20].Id),
                Municipality.Create("251", "RÍO GRANDE (D.M.)",                                     p[20].Id),
                Municipality.Create("252", "MAIMÓN (D.M.)",                                         p[20].Id),
                Municipality.Create("253", "EL ESTRECHO DE LUPERÓN OMAR BROSS (D.M.)",              p[20].Id),
                Municipality.Create("254", "GUALETE (D.M.)",                                        p[20].Id),
                Municipality.Create("255", "VILLA TAPIA",                                           p[21].Id),
                Municipality.Create("256", "TENARES",                                               p[21].Id),
                Municipality.Create("257", "SALCEDO",                                               p[21].Id),
                Municipality.Create("258", "BLANCO (D.M.)",                                         p[21].Id),
                Municipality.Create("259", "JAMAO AFUERA (D.M.)",                                   p[21].Id),
                Municipality.Create("260", "SANTA BARBARA DE SAMANA",                               p[22].Id),
                Municipality.Create("261", "SANCHEZ",                                               p[22].Id),
                Municipality.Create("262", "LAS TERRENAS",                                          p[22].Id),
                Municipality.Create("263", "EL LIMÓN (D.M.)",                                       p[22].Id),
                Municipality.Create("264", "ARROYO BARRIL (D.M.)",                                  p[22].Id),
                Municipality.Create("265", "LAS GALERAS (D.M.)",                                    p[22].Id),
                Municipality.Create("266", "SAN CRISTOBAL",                                         p[23].Id),
                Municipality.Create("267", "VILLA ALTAGRACIA",                                      p[23].Id),
                Municipality.Create("268", "YAGUATE",                                               p[23].Id),
                Municipality.Create("269", "BAJOS DE HAINA",                                        p[23].Id),
                Municipality.Create("270", "SABANA GRANDE DE PALENQUE",                             p[23].Id),
                Municipality.Create("271", "CAMBITA GARABITOS",                                     p[23].Id),
                Municipality.Create("272", "LOS CACAOS (DM)",                                       p[23].Id),
                Municipality.Create("273", "NIGUA (DM)",                                            p[23].Id),
                Municipality.Create("274", "EL CARRIL (D.M.)",                                      p[23].Id),
                Municipality.Create("275", "LA CUCHILLA (D.M.)",                                    p[23].Id),
                Municipality.Create("276", "SAN JOSÉ DEL PUERTO (D.M.)",                            p[23].Id),
                Municipality.Create("277", "MEDINA (D.M.)",                                         p[23].Id),
                Municipality.Create("278", "HATO DAMAS (D.M.)",                                     p[23].Id),
                Municipality.Create("279", "CAMBITA EL PUEBLECITO (D. M.)",                         p[23].Id),
                Municipality.Create("280", "LAS MATAS DE FARFAN",                                   p[24].Id),
                Municipality.Create("281", "SAN JUAN DE LA MAGUANA",                                p[24].Id),
                Municipality.Create("282", "EL CERCADO",                                            p[24].Id),
                Municipality.Create("283", "VALLEJUELO",                                            p[24].Id),
                Municipality.Create("284", "BOHECHIO",                                              p[24].Id),
                Municipality.Create("285", "JUAN DE HERRERA",                                       p[24].Id),
                Municipality.Create("286", "MATAYAYA (D.M.)",                                       p[24].Id),
                Municipality.Create("287", "PEDRO CORTO (D.M.)",                                    p[24].Id),
                Municipality.Create("288", "ARROYO CANO (D.M.)",                                    p[24].Id),
                Municipality.Create("289", "SABANETA (D.M.)",                                       p[24].Id),
                Municipality.Create("290", "YAQUE (D.M.)",                                          p[24].Id),
                Municipality.Create("291", "SABANA ALTA (D.M.)",                                    p[24].Id),
                Municipality.Create("292", "DERRUMBADERO (D.M.)",                                   p[24].Id),
                Municipality.Create("293", "EL ROSARIO (D.M.)",                                     p[24].Id),
                Municipality.Create("294", "HATO DEL PADRE (D.M.)",                                 p[24].Id),
                Municipality.Create("295", "BATISTA (D.M.)",                                        p[24].Id),
                Municipality.Create("296", "CARRERA DE YEGUAS (D.M.)",                              p[24].Id),
                Municipality.Create("297", "GUANITO (D.M.)",                                        p[24].Id),
                Municipality.Create("298", "LA JAGUA (D.M.)",                                       p[24].Id),
                Municipality.Create("299", "JORJILLO (D.M.)",                                       p[24].Id),
                Municipality.Create("300", "LAS MAGUANAS-HATO NUEVO (D.M.)",                        p[24].Id),
                Municipality.Create("301", "LAS CHARCAS DE MARÍA NOVA (D.M.)",                      p[24].Id),
                Municipality.Create("302", "JÍNOVA (D.M.)",                                         p[24].Id),
                Municipality.Create("303", "LAS ZANJAS (D.M.)",                                     p[24].Id),
                Municipality.Create("304", "SAN PEDRO DE MACORIS",                                  p[25].Id),
                Municipality.Create("305", "LOS LLANOS",                                            p[25].Id),
                Municipality.Create("306", "RAMON SANTANA",                                         p[25].Id),
                Municipality.Create("307", "CONSUELO",                                              p[25].Id),
                Municipality.Create("308", "QUISQUEYA",                                             p[25].Id),
                Municipality.Create("309", "EL PUERTO (D.M.)",                                      p[25].Id),
                Municipality.Create("310", "GAUTIER (D.M.)",                                        p[25].Id),
                Municipality.Create("311", "GUAYACANES",                                            p[25].Id),
                Municipality.Create("312", "COTUI",                                                 p[26].Id),
                Municipality.Create("313", "CEVICOS",                                               p[26].Id),
                Municipality.Create("314", "FANTINO",                                               p[26].Id),
                Municipality.Create("315", "LA CUEVA (DM)",                                         p[26].Id),
                Municipality.Create("316", "LA MATA",                                               p[26].Id),
                Municipality.Create("317", "LA BIJA (D.M.)",                                        p[26].Id),
                Municipality.Create("318", "ANGELINA (D.M.)",                                       p[26].Id),
                Municipality.Create("319", "PLATANAL (D.M.)",                                       p[26].Id),
                Municipality.Create("320", "QUITA SUEÑO (D.M.)",                                    p[26].Id),
                Municipality.Create("321", "CABALLERO (D.M.)",                                      p[26].Id),
                Municipality.Create("322", "COMEDERO ARRIBA (D.M.)",                                p[26].Id),
                Municipality.Create("323", "HERNANDO ALONZO (D.M.)",                                p[26].Id),
                Municipality.Create("324", "SANTIAGO DE LOS CABALLEROS",                            p[27].Id),
                Municipality.Create("325", "TAMBORIL",                                              p[27].Id),
                Municipality.Create("326", "JANICO",                                                p[27].Id),
                Municipality.Create("327", "SAN JOSE DE LAS MATAS",                                 p[27].Id),
                Municipality.Create("328", "VILLA GONZALEZ",                                        p[27].Id),
                Municipality.Create("329", "LICEY AL MEDIO",                                        p[27].Id),
                Municipality.Create("330", "VILLA BISONO -NAVARRETE-",                              p[27].Id),
                Municipality.Create("331", "PEDRO GARCÍA (D.M.)",                                   p[27].Id),
                Municipality.Create("332", "SABANA IGLESIA",                                        p[27].Id),
                Municipality.Create("333", "BAITOA (D.M.)",                                         p[27].Id),
                Municipality.Create("334", "LA CANELA (D.M.)",                                      p[27].Id),
                Municipality.Create("335", "EL RUBIO (D.M.)",                                       p[27].Id),
                Municipality.Create("336", "JUNCALITO (D.M.)",                                      p[27].Id),
                Municipality.Create("337", "PALMAR ARRIBA (D.M.)",                                  p[27].Id),
                Municipality.Create("338", "SAN FRANCISCO DE JACAGUA (D.M.)",                       p[27].Id),
                Municipality.Create("339", "EL LIMÓN (D.M.)",                                       p[27].Id),
                Municipality.Create("340", "HATO DEL YAQUE (D.M.)",                                 p[27].Id),
                Municipality.Create("341", "LA CUESTA (D.M.)",                                      p[27].Id),
                Municipality.Create("342", "LAS PLACETAS (D.M.)",                                   p[27].Id),
                Municipality.Create("343", "PUÑAL",                                                 p[27].Id),
                Municipality.Create("344", "GUAYABAL (D.M.)",                                       p[27].Id),
                Municipality.Create("345", "CANABACOA (D.M.)",                                      p[27].Id),
                Municipality.Create("346", "EL CAIMITO (D.M.)",                                     p[27].Id),
                Municipality.Create("347", "LAS PALOMAS (D.M.)",                                    p[27].Id),
                Municipality.Create("348", "CANCA LA PIEDRA (D.M.)",                                p[27].Id),
                Municipality.Create("349", "MONCION",                                               p[28].Id),
                Municipality.Create("350", "SAN IGNACIO DE SABANETA",                               p[28].Id),
                Municipality.Create("351", "VILLA LOS ALMACIGOS",                                   p[28].Id),
                Municipality.Create("352", "MAO",                                                   p[29].Id),
                Municipality.Create("353", "ESPERANZA",                                             p[29].Id),
                Municipality.Create("354", "LAGUNA SALADA",                                         p[29].Id),
                Municipality.Create("355", "AMINA (D.M.)",                                          p[29].Id),
                Municipality.Create("356", "GUATAPANAL (D.M.)",                                     p[29].Id),
                Municipality.Create("357", "JAIBÓN (PUEBLO NUEVO) (D.M.)",                          p[29].Id),
                Municipality.Create("358", "MAIZAL (D.M.)",                                         p[29].Id),
                Municipality.Create("359", "JICOMÉ (D.M.)",                                         p[29].Id),
                Municipality.Create("360", "JAIBÓN (D.M.)",                                         p[29].Id),
                Municipality.Create("361", "LA CAYA (D.M.)",                                        p[29].Id),
                Municipality.Create("362", "CRUCE DE GUAYACANES (D.M.)",                            p[29].Id),
                Municipality.Create("363", "PARADERO (D.M.)",                                       p[29].Id),
                Municipality.Create("364", "BOCA DE MAO (D.M.)",                                    p[29].Id),
                Municipality.Create("365", "SAN JOSE DE OCOA",                                      p[30].Id),
                Municipality.Create("366", "SABANA LARGA",                                          p[30].Id),
                Municipality.Create("367", "RANCHO ARRIBA",                                         p[30].Id),
                Municipality.Create("368", "LA CIÉNAGA (D.M.)",                                     p[30].Id),
                Municipality.Create("369", "NIZAO - LAS AUYAMAS (D.M.)",                            p[30].Id),
                Municipality.Create("370", "EL PINAR (D.M.)",                                       p[30].Id),
                Municipality.Create("371", "EL NARANJAL (D.M.)",                                    p[30].Id),
                Municipality.Create("372", "SANTO DOMINGO ESTE",                                    p[31].Id),
                Municipality.Create("373", "SANTO DOMINGO OESTE",                                   p[31].Id),
                Municipality.Create("374", "SANTO DOMINGO NORTE",                                   p[31].Id),
                Municipality.Create("375", "BOCA CHICA",                                            p[31].Id),
                Municipality.Create("376", "SAN ANTONIO DE GUERRA",                                 p[31].Id),
                Municipality.Create("377", "PEDRO BRAND",                                           p[31].Id),
                Municipality.Create("378", "LOS ALCARRIZOS",                                        p[31].Id),
                Municipality.Create("379", "LA VICTORIA (D.M.)",                                    p[31].Id),
                Municipality.Create("380", "LA CALETA (D.M.)",                                      p[31].Id),
                Municipality.Create("381", "SAN LUÍS (D.M.)",                                       p[31].Id),
                Municipality.Create("382", "HATO VIEJO (D.M.)",                                     p[31].Id),
                Municipality.Create("383", "LA GUÁYIGA (D.M.)",                                     p[31].Id),
                Municipality.Create("384", "LA CUABA (D.M.)",                                       p[31].Id),
                Municipality.Create("385", "PALMAREJO-VILLA LINDA (D.M.)",                          p[31].Id),
                Municipality.Create("386", "PANTOJA (D.M.)",                                        p[31].Id),
            };

            await context.Municipalities.AddRangeAsync(municipalities, ct);
            await context.SaveChangesAsync(ct);
        }
    }
}
