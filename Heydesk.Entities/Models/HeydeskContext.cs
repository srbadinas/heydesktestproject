using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Heydesk.Entities.Models;

public partial class HeydeskContext : DbContext
{
    public HeydeskContext()
    {
    }

    public HeydeskContext(DbContextOptions<HeydeskContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<ConversationMessage> ConversationMessages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Heydesk;Username=postgres;Password=12345");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("conversations_pkey");

            entity.ToTable("conversations");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("timezone('utc'::text, now())")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.IsAiResponse)
                .HasDefaultValueSql("false")
                .HasColumnName("is_ai_response");
            entity.Property(e => e.IsEnded).HasColumnName("is_ended");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("conversations_user_id_fkey");
        });

        modelBuilder.Entity<ConversationMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("conversation_messages_pkey");

            entity.ToTable("conversation_messages");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("timezone('utc'::text, now())")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.RecipientId).HasColumnName("recipient_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");

            entity.HasOne(d => d.Conversation).WithMany(p => p.ConversationMessages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("conversation_messages_conversation_id_fkey");

            entity.HasOne(d => d.Recipient).WithMany(p => p.ConversationMessageRecipients)
                .HasForeignKey(d => d.RecipientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("conversation_messages_recipient_id_fkey");

            entity.HasOne(d => d.Sender).WithMany(p => p.ConversationMessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("conversation_messages_sender_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("timezone('utc'::text, now())")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasMaxLength(255)
                .HasColumnName("firstname");
            entity.Property(e => e.IsOperator).HasColumnName("is_operator");
            entity.Property(e => e.Lastname)
                .HasMaxLength(255)
                .HasColumnName("lastname");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
