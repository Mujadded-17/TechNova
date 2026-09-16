using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TechNova.Models;

namespace TechNova.Services
{
    public class InvestmentPdfService
    {
        public byte[] GenerateInvestmentConfirmation(
            InvestmentRequest request)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string referenceNumber =
                $"INV-{request.RequestDate.Year}-{request.RequestID:D6}";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .AlignCenter()
                                .Text("TECHNOVA")
                                .FontSize(24)
                                .Bold();

                            column.Item()
                                .AlignCenter()
                                .Text("INVESTMENT CONFIRMATION")
                                .FontSize(18)
                                .Bold();

                            column.Item()
                                .AlignCenter()
                                .PaddingTop(5)
                                .Text($"Reference: {referenceNumber}")
                                .FontSize(10);
                        });

                    page.Content()
                        .PaddingTop(30)
                        .Column(column =>
                        {
                            column.Item()
                                .Text("Investment Details")
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .PaddingTop(15)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Cell()
                                        .Padding(8)
                                        .Text("Investor");

                                    table.Cell()
                                        .Padding(8)
                                        .Text(
                                            request.Investor?.Name
                                            ?? "N/A");

                                    table.Cell()
                                        .Padding(8)
                                        .Text("Investor Company");

                                    table.Cell()
                                        .Padding(8)
                                        .Text(
                                            string.IsNullOrWhiteSpace(
                                                request.Investor?.CompanyName)
                                                ? "N/A"
                                                : request.Investor.CompanyName);

                                    table.Cell()
                                        .Padding(8)
                                        .Text("Startup");

                                    table.Cell()
                                        .Padding(8)
                                        .Text(
                                            request.Startup?.CompanyName
                                            ?? "N/A");

                                    table.Cell()
                                        .Padding(8)
                                        .Text("Investment Amount");

                                    table.Cell()
                                        .Padding(8)
                                        .Text(
                                            $"BDT {request.InvestmentAmount:N2}");

                                    table.Cell()
                                        .Padding(8)
                                        .Text("Request Date");

                                    table.Cell()
                                        .Padding(8)
                                        .Text(
                                            request.RequestDate
                                                .ToLocalTime()
                                                .ToString("dd MMMM yyyy"));

                                    table.Cell()
                                        .Padding(8)
                                        .Text("Accepted Date");

                                    table.Cell()
                                        .Padding(8)
                                        .Text(
                                            request.AcceptedDate.HasValue
                                                ? request.AcceptedDate.Value
                                                    .ToLocalTime()
                                                    .ToString("dd MMMM yyyy")
                                                : "N/A");

                                    table.Cell()
                                        .Padding(8)
                                        .Text("Status");

                                    table.Cell()
                                        .Padding(8)
                                        .Text("ACCEPTED")
                                        .Bold();
                                });

                            column.Item()
                                .PaddingTop(30)
                                .Text("Investment Confirmation")
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .PaddingTop(10)
                                .Text(
                                    "This document confirms that the startup "
                                    + "has accepted the investment request "
                                    + "specified above.");

                            column.Item()
                                .PaddingTop(40)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Column(c =>
                                        {
                                            c.Item()
                                                .Text(
                                                    "________________________");

                                            c.Item()
                                                .Text("Investor Signature");
                                        });

                                    row.RelativeItem()
                                        .Column(c =>
                                        {
                                            c.Item()
                                                .Text(
                                                    "________________________");

                                            c.Item()
                                                .Text(
                                                    "Startup Representative");
                                        });
                                });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Generated by TechNova")
                        .FontSize(9);
                });
            });

            return document.GeneratePdf();
        }
    }
}
