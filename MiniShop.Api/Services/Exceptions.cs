namespace MiniShop.Api.Services;

public class NotFoundException(string message) : Exception(message);

public class ValidationException(string message) : Exception(message);