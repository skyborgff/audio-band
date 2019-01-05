﻿using System.ServiceModel;
using System.Threading.Tasks;
using AudioBand.AudioSource;

namespace ServiceContracts
{
    /// <summary>
    /// Contract for the service that is hosting the audio sources.
    /// </summary>
    [ServiceContract(Namespace = "Audioband", CallbackContract = typeof(IAudioSourceHostCallback))]
    public interface IAudioSourceHost
    {
        /// <summary>
        /// Ping to check if the server is still responding.
        /// </summary>
        [OperationContract]
        void IsAlive();

        /// <summary>
        /// Contract method for <see cref="IAudioSource.Name"/>.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string GetName();

        /// <summary>
        /// Contract method for <see cref="IAudioSource.ActivateAsync(System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        Task ActivateAsync();

        /// <summary>
        /// Contract method for <see cref="IAudioSource.DeactivateAsync(System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        Task DeactivateAsync();

        /// <summary>
        /// Contract method for <see cref="IAudioSource.PlayTrackAsync(System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        Task PlayTrackAsync();

        /// <summary>
        /// Contract method for <see cref="IAudioSource.PauseTrackAsync(System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        Task PauseTrackAsync();

        /// <summary>
        /// Contract method for <see cref="IAudioSource.PreviousTrackAsync(System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        Task PreviousTrackAsync();

        /// <summary>
        /// Contract method for <see cref="IAudioSource.NextTrackAsync(System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        Task NextTrackAsync();
    }
}