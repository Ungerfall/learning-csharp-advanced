using System.Threading.Tasks;

namespace Data
{
	public interface IRepositoryAsync<T>
	{
		Task<T> CreateAsync(T entity);
		Task<T> ReadAsync(T entity);
		Task<T> UpdateAsync(T entity);
		Task DeleteAsync(T entity);
	}
}
