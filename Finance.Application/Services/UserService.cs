using AutoMapper;
using Finance.Application.Exceptions;
using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using Finance.Domain.Models.Enums;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Finance.Application.Services
{
    public class UserService : BaseService<User, UserCreateDto, UserSelectDto>, IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;

        // Verified when the email is unknown so both failure paths cost a BCrypt verification.
        private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword("dummy-password-for-timing");

        public UserService(
            IBaseRepository<User, int> repository,
            IUserRepository userRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<UserCreateDto> validator,
            ICategoryRepository categoryRepository) : base(repository, uow, mapper, validator)
        {
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        public async Task<UserSelectDto> RegisterAsync(UserCreateDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var email = NormalizeEmail(dto.Email);
            var taken = new ConflictException("EMAIL_TAKEN", "Email is already registered");

            if (await _userRepository.GetByEmailAsync(email) != null)
                throw taken;

            var userName = dto.UserName.Trim();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            try
            {
                return await _uow.ExecuteInTransactionAsync(async () =>
                {
                    // Created inside the delegate: it may run more than once (connection retry).
                    // Role is never taken from the request: public registration always creates a regular user.
                    var user = new User
                    {
                        UserName = userName,
                        Email = email,
                        Password = passwordHash,
                        Role = EUserRole.user
                    };
                    await _userRepository.AddAsync(user);
                    await _uow.CommitAsync(); // assigns user.Id

                    foreach (var name in DefaultCategories.Names)
                        await _categoryRepository.AddAsync(new Category { Name = name, UserId = user.Id });
                    await _uow.CommitAsync();

                    return _mapper.Map<UserSelectDto>(user);
                });
            }
            catch (UniqueConstraintException)
            {
                // Lost a race with a concurrent registration of the same email.
                throw taken;
            }
        }

        public async Task DeleteAccountAsync(int userId)
        {
            var user = await _repository.GetByIdAsync(userId) ?? throw new KeyNotFoundException("User not found");
            // The database cascades the user's categories and movements.
            await _repository.DeleteAsync(user.Id);
            await _uow.CommitAsync();
        }
        public override async Task<UserSelectDto> AddAsync(UserCreateDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var user = _mapper.Map<User>(dto);
            user.Email = NormalizeEmail(user.Email);
            user.Role = EUserRole.user;

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            await _userRepository.AddAsync(user);
            await _uow.CommitAsync();

            return _mapper.Map<UserSelectDto>(user);
        }

        public override async Task<UserSelectDto> UpdateAsync(int id, UserCreateDto dto)
        {
            var existing = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("User not found");

            // Map onto the tracked entity: Id and Role (not in the DTO) are preserved.
            _mapper.Map(dto, existing);
            existing.Email = NormalizeEmail(existing.Email);
            existing.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _repository.UpdateAsync(id, existing);
            await _uow.CommitAsync();

            return _mapper.Map<UserSelectDto>(existing);
        }

        public async Task<UserSelectDto?> ValidLoginUserAsync(string email, string password)
        {
                var user = await _userRepository.GetByEmailAsync(NormalizeEmail(email));

                if (user == null)
                {
                    BCrypt.Net.BCrypt.Verify(password, DummyHash);
                    return null;
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                    return null;

                return _mapper.Map<UserSelectDto>(user);
        }
    }
}
