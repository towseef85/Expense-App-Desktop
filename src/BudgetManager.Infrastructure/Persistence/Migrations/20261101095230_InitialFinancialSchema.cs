using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFinancialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<byte[]>(type: "BLOB", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    kind = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    parent_id = table.Column<byte[]>(type: "BLOB", nullable: true),
                    archived_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    row_version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.id);
                    table.CheckConstraint("ck_category_kind", "kind IN ('Income','Expense')");
                    table.CheckConstraint("ck_category_row_version", "row_version >= 0");
                    table.ForeignKey(
                        name: "FK_category_category_parent_id",
                        column: x => x.parent_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "currency",
                columns: table => new
                {
                    code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    minor_unit_digits = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency", x => x.code);
                    table.CheckConstraint("ck_currency_code", "length(code) = 3 AND code = upper(code)");
                    table.CheckConstraint("ck_currency_minor_unit_digits", "minor_unit_digits BETWEEN 0 AND 3");
                });

            migrationBuilder.CreateTable(
                name: "financial_transaction",
                columns: table => new
                {
                    id = table.Column<byte[]>(type: "BLOB", nullable: false),
                    kind = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    effective_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    created_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    modified_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    posted_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    voided_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    correction_of_transaction_id = table.Column<byte[]>(type: "BLOB", nullable: true),
                    reconciliation_id = table.Column<byte[]>(type: "BLOB", nullable: true),
                    split_base_rounding_difference_minor = table.Column<long>(type: "INTEGER", nullable: false),
                    is_savings_transfer = table.Column<bool>(type: "INTEGER", nullable: false),
                    row_version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_transaction", x => x.id);
                    table.CheckConstraint("ck_financial_transaction_kind", "kind IN ('Income','Expense','Transfer','OpeningBalance','BalanceAdjustment')");
                    table.CheckConstraint("ck_financial_transaction_row_version", "row_version >= 0");
                    table.CheckConstraint("ck_financial_transaction_status", "status IN ('Draft','Posted','Voided')");
                    table.ForeignKey(
                        name: "FK_financial_transaction_financial_transaction_correction_of_transaction_id",
                        column: x => x.correction_of_transaction_id,
                        principalTable: "financial_transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    id = table.Column<byte[]>(type: "BLOB", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    account_type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    currency_code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    is_savings = table.Column<bool>(type: "INTEGER", nullable: false),
                    archived_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    row_version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account", x => x.id);
                    table.CheckConstraint("ck_account_row_version", "row_version >= 0");
                    table.CheckConstraint("ck_account_type", "account_type IN ('Cash','Checking','Savings','CreditCard','EWallet','Loan','Other')");
                    table.ForeignKey(
                        name: "FK_account_currency_currency_code",
                        column: x => x.currency_code,
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transaction_split",
                columns: table => new
                {
                    id = table.Column<byte[]>(type: "BLOB", nullable: false),
                    transaction_id = table.Column<byte[]>(type: "BLOB", nullable: false),
                    category_id = table.Column<byte[]>(type: "BLOB", nullable: false),
                    amount_minor = table.Column<long>(type: "INTEGER", nullable: false),
                    currency_code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    base_amount_minor = table.Column<long>(type: "INTEGER", nullable: false),
                    base_currency_code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    quote_units_per_base = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_split", x => x.id);
                    table.CheckConstraint("ck_transaction_split_positive", "amount_minor > 0");
                    table.CheckConstraint("ck_transaction_split_rate", "quote_units_per_base > 0");
                    table.ForeignKey(
                        name: "FK_transaction_split_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_split_currency_base_currency_code",
                        column: x => x.base_currency_code,
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_split_currency_currency_code",
                        column: x => x.currency_code,
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_split_financial_transaction_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "financial_transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction_leg",
                columns: table => new
                {
                    id = table.Column<byte[]>(type: "BLOB", nullable: false),
                    transaction_id = table.Column<byte[]>(type: "BLOB", nullable: false),
                    account_id = table.Column<byte[]>(type: "BLOB", nullable: false),
                    amount_minor = table.Column<long>(type: "INTEGER", nullable: false),
                    currency_code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    base_amount_minor = table.Column<long>(type: "INTEGER", nullable: false),
                    base_currency_code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    quote_units_per_base = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_leg", x => x.id);
                    table.CheckConstraint("ck_transaction_leg_nonzero", "amount_minor <> 0");
                    table.CheckConstraint("ck_transaction_leg_rate", "quote_units_per_base > 0");
                    table.ForeignKey(
                        name: "FK_transaction_leg_account_account_id",
                        column: x => x.account_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_leg_currency_base_currency_code",
                        column: x => x.base_currency_code,
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_leg_currency_currency_code",
                        column: x => x.currency_code,
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_leg_financial_transaction_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "financial_transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_currency_code",
                table: "account",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ux_account_active_name",
                table: "account",
                column: "name",
                unique: true,
                filter: "archived_utc IS NULL");

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX ux_category_active_sibling
                ON category (ifnull(hex(parent_id), ''), name, kind)
                WHERE archived_utc IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_category_parent_id",
                table: "category",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_financial_transaction_correction_of_transaction_id",
                table: "financial_transaction",
                column: "correction_of_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_financial_transaction_effective_date_status",
                table: "financial_transaction",
                columns: new[] { "effective_date", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_transaction_leg_account_transaction",
                table: "transaction_leg",
                columns: new[] { "account_id", "transaction_id" });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_leg_base_currency_code",
                table: "transaction_leg",
                column: "base_currency_code");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_leg_currency_code",
                table: "transaction_leg",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_leg_transaction_id",
                table: "transaction_leg",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_split_base_currency_code",
                table: "transaction_split",
                column: "base_currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_split_category_transaction",
                table: "transaction_split",
                columns: new[] { "category_id", "transaction_id" });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_split_currency_code",
                table: "transaction_split",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_split_transaction_id",
                table: "transaction_split",
                column: "transaction_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transaction_leg");

            migrationBuilder.DropTable(
                name: "transaction_split");

            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "financial_transaction");

            migrationBuilder.DropTable(
                name: "currency");
        }
    }
}
