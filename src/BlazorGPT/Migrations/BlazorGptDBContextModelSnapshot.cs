﻿// <auto-generated />
using System;
using BlazorGPT.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BlazorGPT.Migrations
{
    [DbContext(typeof(BlazorGptDBContext))]
    partial class BlazorGptDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BlazorGPT.Data.Model.Conversation", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("BranchedFromMessageId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DateStarted")
                        .HasColumnType("datetime2");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Summary")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BranchedFromMessageId");

                    b.ToTable("Conversations");
                });

            modelBuilder.Entity("BlazorGPT.Data.Model.ConversationMessage", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("CompletionTokens")
                        .HasColumnType("int");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "content");

                    b.Property<Guid?>("ConversationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "name");

                    b.Property<int?>("PromptTokens")
                        .HasColumnType("int");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "role");

                    b.HasKey("Id");

                    b.HasIndex("ConversationId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("BlazorGPT.Data.Model.ConversationQuickProfile", b =>
                {
                    b.Property<Guid>("ConversationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("QuickProfileId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ConversationId", "QuickProfileId");

                    b.HasIndex("QuickProfileId");

                    b.ToTable("ConversationQuickProfiles", (string)null);
                });

            modelBuilder.Entity("BlazorGPT.Data.Model.Script", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SystemMessage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Scripts");
                });

            modelBuilder.Entity("BlazorGPT.Data.Model.ScriptStep", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ScriptId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ScriptId");

                    b.ToTable("ScriptSteps");
                });

            modelBuilder.Entity("BlazorGPT.Data.QuickProfile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EnabledDefault")
                        .HasColumnType("bit");

                    b.Property<int>("InsertAt")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("QuickProfiles");
                });

            modelBuilder.Entity("BlazorGPT.Data.Model.Conversation", b =>
                {
                    b.HasOne("BlazorGPT.Data.Model.ConversationMessage", "BranchedFromMessage")
                        .WithMany("BranchedConversations")
                        .HasForeignKey("BranchedFromMessageId")
                        .OnDelete(DeleteBehavior.ClientNoAction);

                    b.OwnsMany("BlazorGPT.Data.Model.ConversationTreeState", "TreeStateList", b1 =>
                        {
                            b1.Property<Guid?>("ConversationId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<Guid?>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("Content")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<bool>("IsPublished")
                                .HasColumnType("bit");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Type")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("ConversationId", "Id");

                            b1.ToTable("TreeStateData");

                            b1.WithOwner("Conversation")
                                .HasForeignKey("ConversationId");

                            b1.Navigation("Conversation");
                        });

                    b.OwnsOne("BlazorGPT.Data.Model.HiveState", "HiveState", b1 =>
                        {
                            b1.Property<Guid?>("ConversationId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("Content")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<Guid?>("Id")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<bool>("IsPublished")
                                .HasColumnType("bit");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Type")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("ConversationId");

                            b1.ToTable("Conversations");

                            b1.WithOwner("Conversation")
                                .HasForeignKey("ConversationId");

                            b1.Navigation("Conversation");
                        });

                    b.Navigation("BranchedFromMessage");

                    b.Navigation("HiveState");

                    b.Navigation("TreeStateList");
                });

            modelBuilder.Entity("BlazorGPT.Data.Model.ConversationMessage", b =>
                {
                    b.HasOne("BlazorGPT.Data.Model.Conversation", "Conversation")
                        .WithMany("Messages")
                        .HasForeignKey("ConversationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.OwnsOne("BlazorGPT.Data.Model.MessageState", "State", b1 =>
                        {
                            b1.Property<Guid?>("MessageId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("Content")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<Guid?>("Id")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<bool>("IsPublished")
                                .HasColumnType("bit");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Type")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("MessageId");

                            b1.ToTable("StateData");

                            b1.WithOwner("Message")
                                .HasForeignKey("MessageId");

                            b1.Navigation("Message");
                        });

                    b.Navigation("Conversation");

                    b.Navigation("State");
                });

            modelBuilder.Entity("BlazorGPT.Data.Model.ConversationQuickProfile", b =>
                {
                    b.HasOne("BlazorGPT.Data.Model.Conversation", "Conversation")
                        .WithMany()
                        .HasForeignKey("ConversationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlazorGPT.Data.QuickProfile", "QuickProfile")
                        .WithMany()
                        .HasForeignKey("QuickProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Conversation");

                    b.Navigation("QuickProfile");
                });

            modelBuilder.Entity("BlazorGPT.Data.Model.ScriptStep", b =>
                {
                    b.HasOne("BlazorGPT.Data.Model.Script", "Script")
                        .WithMany("Steps")
                        .HasForeignKey("ScriptId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Script");
                });

            modelBuilder.Entity("BlazorGPT.Data.Model.Conversation", b =>
                {
                    b.Navigation("Messages");
                });

            modelBuilder.Entity("BlazorGPT.Data.Model.ConversationMessage", b =>
                {
                    b.Navigation("BranchedConversations");
                });

            modelBuilder.Entity("BlazorGPT.Data.Model.Script", b =>
                {
                    b.Navigation("Steps");
                });
#pragma warning restore 612, 618
        }
    }
}
