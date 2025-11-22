using AutoMapper;
using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using FluentValidation;

namespace Finance.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IValidator<UserCreateDto> _validator;

        public UserService(
            IUserRepository repository,
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<UserCreateDto> validator)
        {
            _repository = repository;
            _uow = uow;
            _mapper = mapper;
            _validator = validator;
        }
        public async Task<UserSelectDto> AddAsync(UserCreateDto dto)
        {
            var validationResult = _validator.Validate(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            var user = _mapper.Map<User>(dto);

            await _repository.AddAsync(_mapper.Map<User>(user));
            await _uow.CommitAsync();

            return _mapper.Map<UserSelectDto>(user);
        }

        public async Task DeleteAsync(int IdTransaction)
        {
            var user = _repository.GetByIdAsync(IdTransaction) ?? throw new NotImplementedException("User not found");

            await _repository.DeleteAsync(IdTransaction);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<UserSelectDto>> GetAllAsync()
        {
            var users = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserSelectDto>>(users);
        }

        public async Task<UserSelectDto> GetByIdAsync(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            return _mapper.Map<UserSelectDto>(user);
        }

        public async Task<UserSelectDto> UpdateAsync(int IdTransaction, UserCreateDto dto)
        {
            var existingUser = _repository.GetByIdAsync(IdTransaction) ?? throw new NotImplementedException("User not found");

            var user = _mapper.Map<User>(dto);
            await _repository.UpdateAsync(IdTransaction, user);
            await _uow.CommitAsync();

            return _mapper.Map<UserSelectDto>(user);
        }

        public async Task<UserSelectDto> ValidLoginUser(string email, string password)
        {
            var user = await _repository.ValidLoginUser(email, password);

            return _mapper.Map<UserSelectDto>(user);
        }
    }
}
