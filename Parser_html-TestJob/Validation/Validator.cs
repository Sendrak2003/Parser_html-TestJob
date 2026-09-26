using FluentValidation;

public class ElementsRequestValidator : AbstractValidator<Payload>
{
    public ElementsRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Selector)
            .NotNull().WithErrorCode(nameof(ErrorType.MissingParameter)).WithMessage("Parameter 'selector' is missing.")
            .NotEmpty().WithErrorCode(nameof(ErrorType.EmptySelector)).WithMessage("Parameter 'selector' must not be empty.");

        RuleFor(x => x.Attribute)
            .NotNull().WithErrorCode(nameof(ErrorType.MissingParameter)).WithMessage("Parameter 'attribute' is missing.")
            .NotEmpty().WithErrorCode(nameof(ErrorType.EmptyAttribute)).WithMessage("Parameter 'attribute' must not be empty.");

        RuleFor(x => x.UrlB64)
            .NotNull().WithErrorCode(nameof(ErrorType.MissingParameter)).WithMessage("Parameter 'url_b64' is missing.")
            .NotEmpty().WithErrorCode(nameof(ErrorType.InvalidUrlBase64)).WithMessage("Parameter 'url_b64' must not be empty.");

        RuleFor(x => x.EncryptedTextBytesB64)
            .NotNull().WithErrorCode(nameof(ErrorType.MissingParameter)).WithMessage("Parameter 'encrypted_text_bytes_b64' is missing.")
            .NotEmpty().WithErrorCode(nameof(ErrorType.InvalidEncryptedTextBytes)).WithMessage("Parameter 'encrypted_text_bytes_b64' must not be empty.");

        RuleFor(x => x.KeyBytesB64)
            .NotNull().WithErrorCode(nameof(ErrorType.MissingParameter)).WithMessage("Parameter 'key_bytes_b64' is missing.")
            .NotEmpty().WithErrorCode(nameof(ErrorType.InvalidKeyBytes)).WithMessage("Parameter 'key_bytes_b64' must not be empty.");

        RuleFor(x => x.PageB64)
            .NotNull().WithErrorCode(nameof(ErrorType.MissingParameter)).WithMessage("Parameter 'page_b64' is missing.")
            .NotEmpty().WithErrorCode(nameof(ErrorType.InvalidPageBase64)).WithMessage("Parameter 'page_b64' must not be empty.");
    }
}
