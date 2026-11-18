using AutoMapper;
using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Finance.Application.Services
{
    public class UserService : BaseService<User, UserCreateDto, UserSelectDto>, IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(
            IBaseRepository<User, int> repository,
            IUserRepository userRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<UserCreateDto> validator) : base(repository, uow, mapper, validator)
        {
            _userRepository = userRepository;
        }
        public override async Task<UserSelectDto> AddAsync(UserCreateDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var user = _mapper.Map<User>(dto);


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
            existing.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _repository.UpdateAsync(id, existing);
            await _uow.CommitAsync();

            return _mapper.Map<UserSelectDto>(existing);
        }

        public async Task<UserSelectDto?> ValidLoginUserAsync(string email, string password)
        {
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                    return null;

                bool isValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

                if (!isValid)
                    return null;

                return _mapper.Map<UserSelectDto>(user);
        }
    }
}
