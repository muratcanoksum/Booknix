﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booknix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IpApprovedTimeField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "TrustedIps",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "TrustedIps");
        }
    }
}
