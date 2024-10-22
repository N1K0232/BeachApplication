namespace BeachApplication.Shared.Models.Requests;

public record class SaveCommentRequest(int Score, string Title, string Text);