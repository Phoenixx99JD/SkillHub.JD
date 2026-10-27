using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkillHub.Application.Abstractions.Services;

namespace SkillHub.Infrastructure.Services;

public class QuestPdfCertificateGenerator : ICertificateGenerator
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");
    private const string Accent = "#1F4E79";

    static QuestPdfCertificateGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(EnrollmentCertificateData data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.DefaultTextStyle(t => t.FontSize(12));

                page.Header().Column(col =>
                {
                    col.Item().Text("SkillHub").FontSize(28).Bold().FontColor(Accent);
                    col.Item().Text("Training center").FontSize(11).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(10).LineHorizontal(2).LineColor(Accent);
                });

                page.Content().PaddingVertical(30).Column(col =>
                {
                    col.Spacing(14);
                    col.Item().AlignCenter().Text("Constancia de inscripción").FontSize(22).Bold();
                    col.Item().AlignCenter().Text("Enrollment certificate").FontSize(12).Italic()
                        .FontColor(Colors.Grey.Darken1);

                    col.Item().PaddingTop(10).Text(text =>
                    {
                        text.Span("SkillHub certifies that ");
                        text.Span(data.ParticipantName).Bold();
                        text.Span($" ({data.ParticipantEmail}) is enrolled in the following workshop:");
                    });

                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(15).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(150);
                            c.RelativeColumn();
                        });

                        void Row(string label, string value)
                        {
                            table.Cell().PaddingVertical(4).Text(label).SemiBold().FontColor(Accent);
                            table.Cell().PaddingVertical(4).Text(value);
                        }

                        Row("Participant", data.ParticipantName);
                        Row("Email", data.ParticipantEmail);
                        Row("Workshop code", data.WorkshopCode);
                        Row("Workshop", data.WorkshopTitle);
                        Row("Instructor", data.InstructorName);
                        Row("Date", data.WorkshopStartsAt.ToString("dddd, MMMM d, yyyy", Culture));
                        Row("Schedule", $"{data.WorkshopStartsAt.ToString("h:mm tt", Culture)} - " +
                                        $"{data.WorkshopEndsAt.ToString("h:mm tt", Culture)}" +
                                        (data.WorkshopEndsAt.Date != data.WorkshopStartsAt.Date
                                            ? $" ({data.WorkshopEndsAt.ToString("MMM d, yyyy", Culture)})"
                                            : string.Empty));
                        Row("Enrollment date", data.EnrolledAt.ToString("MMMM d, yyyy h:mm tt", Culture));
                    });
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text($"Generated on {data.GeneratedAt.ToString("MMMM d, yyyy h:mm tt", Culture)}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text($"Enrollment ID: {data.EnrollmentId}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }).GeneratePdf();
    }
}
