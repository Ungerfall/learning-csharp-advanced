using System.Threading.Tasks;

namespace Data
{
	public abstract class RepositoryAsyncBase<T> : IRepositoryAsync<T>
	{
		protected readonly IDataProvider syncDataProvider;

		public RepositoryAsyncBase(IDataProvider syncDataProvider)
		{
			if (syncDataProvider == null) throw new System.ArgumentNullException(nameof(syncDataProvider));

			this.syncDataProvider = syncDataProvider;
		}

		protected abstract string CreateCommand { get; }

		protected abstract string ReadCommand { get; }

		protected abstract string UpdateCommand { get; }

		protected abstract string DeleteCommand { get; }

		public async Task<T> CreateAsync(T entity)
		{
			return await Task.Factory.StartNew(() =>
			{
				return (T)syncDataProvider.Execute(CreateCommand, entity);
			});
		}

		public async Task<T> ReadAsync(T entity)
		{
			return await Task.Factory.StartNew(() =>
			{
				return (T)syncDataProvider.Execute(ReadCommand, entity);
			});
		}

		public async Task<T> UpdateAsync(T entity)
		{
			return await Task.Factory.StartNew(() =>
			{
				return (T)syncDataProvider.Execute(UpdateCommand, entity);
			});
		}

		public async Task DeleteAsync(T entity)
		{
			await Task.Factory.StartNew(() =>
			{
				syncDataProvider.Execute(DeleteCommand, entity);
			});
		}
	}
}
