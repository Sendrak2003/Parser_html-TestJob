using FluentValidation;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/elements")]
public class ElementsController : ControllerBase
{
    private readonly IValidator<Payload> _validator;
    private readonly ParserService _service;

    public ElementsController(IValidator<Payload> validator, ParserService service)
    {
        _validator = validator;
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Process(Payload request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var error = validation.Errors.First();
            var code = Enum.TryParse<ErrorType>(error.ErrorCode, out var parsed) ? parsed : ErrorType.UnknownError;
            return BadRequest(new Result { IsError = 1, ErrorCode = code, ErrorMessage = error.ErrorMessage });
        }

        try
        {
            return Ok(await _service.ProcessAsync(request));
        }
        catch (CustomError ex)
        {
            return BadRequest(new Result { IsError = 1, ErrorCode = ex.Code, ErrorMessage = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Result { IsError = 1, ErrorCode = ErrorType.UnknownError, ErrorMessage = ex.Message });
        }
    }
}
