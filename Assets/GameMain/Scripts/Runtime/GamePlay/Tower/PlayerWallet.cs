using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>金币账本。支付凭据只能退一次；保留退款额度以避免余额溢出。</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerWallet : MonoBehaviour
    {
        [SerializeField, Min(0)] private int initialCoins = 500;
        private readonly Dictionary<Guid, int> payments = new Dictionary<Guid, int>();
        private readonly HashSet<Guid> usedTransactions = new HashSet<Guid>();
        private long refundable;
        public long Balance { get; private set; }
        public event Action<long> BalanceChanged;

        private void Awake() => Balance = Math.Max(0, initialCoins);

        public bool TrySpend(Guid transaction, int amount)
        {
            if (transaction == Guid.Empty || amount < 0 || amount > Balance || !usedTransactions.Add(transaction)) return false;
            payments.Add(transaction, amount);
            Balance -= amount;
            refundable += amount;
            Notify();
            return true;
        }

        public bool Refund(Guid transaction)
        {
            if (!payments.TryGetValue(transaction, out int amount)) return false;
            payments.Remove(transaction);
            refundable -= amount;
            Balance += amount;
            Notify();
            return true;
        }

        /// <summary>关卡清理时注销已完成支付的退款权，不返还金币。</summary>
        public void ForgetPayment(Guid transaction)
        {
            if (!payments.TryGetValue(transaction, out int amount)) return;
            payments.Remove(transaction);
            refundable -= amount;
        }

        public bool TryAdd(long amount)
        {
            if (amount < 0 || amount > long.MaxValue - Balance - refundable) return false;
            Balance += amount;
            Notify();
            return true;
        }

        private void Notify()
        {
            if (BalanceChanged == null) return;
            foreach (Action<long> listener in BalanceChanged.GetInvocationList())
                try { listener(Balance); } catch (Exception e) { Debug.LogException(e); }
        }
    }
}
