/*  
  Copyright 2007-2017 The NGenerics Team
 (https://github.com/ngenerics/ngenerics/wiki/Team)

 This program is licensed under the MIT License.  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at https://opensource.org/licenses/MIT.
*/



using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NGenerics.Util;

namespace NGenerics.DataStructures.General
{
	/// <summary>
	/// A Bag data structure.
	/// </summary>
	/// <remarks>
    /// A Bag (sometimes also called a multiset) is a group of object in which each 
    /// each member has a multiplicity, which is a natural number indicating (loosely speaking)
    /// how many times it is a member. For example, in the bag { a, a, b, b, b, c }, the 
    /// multiplicities of the members a, b, and c are respectively 2, 3, and 1.
	/// </remarks>
	/// <typeparam name="T">The type of elements in the <see cref="Bag{T}"/>.</typeparam>
    [Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public class Bag<T> : IBag<T>, IEquatable<Bag<T>>
	{
		#region Globals

		private readonly Dictionary<T, int> _data;

	    #endregion

		#region Construction

		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="Constructor" lang="cs" title="The following example shows how to use the default constructor."/>
		/// </example>
		public Bag()
		{
            _data = new Dictionary<T, int>();
		}

		/// <param name="capacity">The initial capacity of the bag.</param>
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="ConstructorCapacity" lang="cs" title="The following example shows how to use the capacity constructor."/>
		/// </example>
		public Bag(int capacity)
		{
            _data = new Dictionary<T, int>(capacity);
		}


        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when testing equality of items in the <see cref="Bag{T}"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
		/// <example>
        /// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="ConstructorComparer" lang="cs" title="The following example shows how to use the comparer constructor."/>
		/// </example>
		public Bag(IEqualityComparer<T> comparer)
		{
            Guard.ArgumentNotNull(comparer, nameof(comparer));
            _data = new Dictionary<T, int>(comparer);
		}



		/// <param name="capacity">The initial capacity of the bag.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when testing equality of items in the <see cref="Bag{T}"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
		public Bag(int capacity, IEqualityComparer<T> comparer)
        {
            Guard.ArgumentNotNull(comparer, nameof(comparer));
            _data = new Dictionary<T, int>(capacity, comparer);
		}


		/// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> to copy values from.</param>
		private Bag(IDictionary<T, int> dictionary)
		{
			Debug.Assert(dictionary != null);

            _data = new Dictionary<T, int>(dictionary);

			// Update the count
			foreach (var item in _data)
			{
				Count += item.Value;
			}
		}

		#endregion

		#region Public Members		

		/// <summary>
		/// Removes all instances of  the specified item in the <see cref="Bag{T}"/>.
		/// </summary>
		/// <param name="item">The <paramref name="item"/> to be removed.</param>
        /// <returns>A value indicating if <paramref name="item"/> was found (and removed) from the <see cref="Bag{T}"/>.</returns>
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="RemoveAll" lang="cs" title="The following example shows how to use the RemoveAll method."/>
		/// </example>
		public bool RemoveAll(T item)
		{
		    if (_data.TryGetValue(item, out var itemCount))
			{
				RemoveItem(item, itemCount, itemCount);
				return true;
			}
		    return false;
		}


		/// <exception cref="ArgumentOutOfRangeException">Maximum is smaller or equal to 0.</exception>
		/// <inheritdoc />
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="RemoveMax" lang="cs" title="The following example shows how to use the Remove method."/>
		/// </example>        
		public bool Remove(T item, int maximum)
		{
			if (maximum <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maximum));
			}

		    if (_data.TryGetValue(item, out var itemCount))
			{
				RemoveItem(item, maximum, itemCount);
				return true;
			}
		    
            return false;
		}	
		
		/// <summary>
		/// Removes the specified amount of items from the <see cref="Bag{T}"/>.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="maximum">The maximum amount of items to remove.</param>
		/// <param name="itemCount">The count of the items being removed.</param>
		/// <remarks>
		/// <b>Notes to Inheritors: </b>
		///  Derived classes can override this method to change the behavior of the <see cref="Remove(T)"/> method.
		/// </remarks>
		protected virtual void RemoveItem(T item, int maximum, int itemCount)
		{
			if (maximum >= itemCount)
			{
				Count -= itemCount;
				_data.Remove(item);
			}
			else
			{
				Count -= maximum;
				_data[item] = itemCount - maximum;
			}
		}


		/// <exception cref="ArgumentOutOfRangeException">The amount of items to b added can only be 1 or more.</exception>
		/// <inheritdoc />
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="AddAmount" lang="cs" title="The following example shows how to use the Add method."/>
		/// </example>
		public void Add(T item, int amount)
		{
			#region Validation

			if (amount <= 0)
			{
                throw new ArgumentOutOfRangeException(nameof(amount), "You can only add 1 or more items.");
			}

			#endregion

			AddItem(item, amount);
		}


		/// <summary>
		/// Adds n * the specified item to the <see cref="Bag{T}"/>.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="amount">The amount.</param>
		/// <remarks>
		/// <b>Notes to Inheritors: </b>
		///  Derived classes can override this method to change the behavior of the <see cref="Add(T)"/> method.
		/// </remarks>
		protected virtual void AddItem(T item, int amount)
		{
		    if (_data.TryGetValue(item, out var itemCount))
			{
				_data[item] = itemCount + amount;
			}
			else
			{
				_data.Add(item, amount);
			}

			Count += amount;
		}


		/// <inheritdoc />
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="GetEnumerator" lang="cs" title="The following example shows how to use the GetEnumerator method."/>
		/// </example>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public IEnumerator<KeyValuePair<T, int>> GetCountEnumerator()
		{
			return _data.GetEnumerator();
		}


		/// <summary>
		/// Computes the union of this <see cref="Bag{T}"/> and the specified <paramref name="bag"/>.
		/// </summary>
		/// <param name="bag">The bag.</param>
        /// <returns>The union of this <see cref="Bag{T}"/> and <paramref name="bag"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bag"/> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="Union" lang="cs" title="The following example shows how to use the Union method."/>
		/// </example>
		public Bag<T> Union(Bag<T> bag)
		{
            return UnionInternal(bag);
		}


		/// <summary>
        /// Computes the difference between this bag and the specified <paramref name="bag"/>.
		/// </summary>
		/// <param name="bag">The bag.</param>
        /// <returns>The difference between this bag and <paramref name="bag"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bag"/> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="Subtract" lang="cs" title="The following example shows how to use the Subtract method."/>
		/// </example>
		public Bag<T> Subtract(Bag<T> bag)
		{
            return SubtractInternal(bag);
		}


		/// <summary>
        /// Computes the intersection between this <see cref="Bag{T}"/> and the specified <paramref name="bag"/>.
		/// </summary>
		/// <param name="bag">The bag.</param>
        /// <returns>The intersection between this <see cref="Bag{T}"/> and <paramref name="bag"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bag"/> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="Intersection" lang="cs" title="The following example shows how to use the Intersection method."/>
		/// </example>
		public Bag<T> Intersection(Bag<T> bag)
		{
            return IntersectionInternal(bag);
		}


	

		#endregion

		#region Private Members

        /// <summary>
        /// Internal method for the Intersection operation.
        /// </summary>
        /// <param name="bag">The bag to perform the intersection on.</param>
        /// <returns>The result of the intersection.</returns>
        private Bag<T> IntersectionInternal(IBag<T> bag)
        {
            Guard.ArgumentNotNull(bag, "bag");


            var resultBag = new Bag<T>();

            using (var enumerator = bag.GetCountEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;

                    if (_data.TryGetValue(item.Key, out var itemCount))
                    {
                        resultBag.Add(item.Key,
                                   Math.Min(item.Value, itemCount)
                            );
                    }
                }
            }

            return resultBag;
        }

        /// <summary>
        /// Internal method for the Union operation.
        /// </summary>
        /// <param name="bag">The bag to perform the union with.</param>
        /// <returns>The result of the union operation.</returns>
        private Bag<T> UnionInternal(IBag<T> bag)
        {
            Guard.ArgumentNotNull(bag, "bag");

            var resultBag = new Bag<T>();

            foreach (var item in _data)
            {
                resultBag.Add(item.Key, item.Value);
            }

            using (var enumerator = bag.GetCountEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;

                    resultBag.Add(item.Key, item.Value);
                }
            }

            return resultBag;
        }

        /// <summary>
        /// Internal method for the subtraction operation.
        /// </summary>
        /// <param name="bag">The bag to subtract from this bag.</param>
        /// <returns>The result of the subtract operation.</returns>
        private Bag<T> SubtractInternal(IBag<T> bag)
        {
            Guard.ArgumentNotNull(bag, "bag");

            var resultBag = new Bag<T>(_data);
            
            using (var enumerator = bag.GetCountEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;

                    if (resultBag._data.TryGetValue(item.Key, out var itemCount))
                    {
                        if (itemCount - item.Value <= 0)
                        {
                            resultBag.RemoveAll(item.Key);
                        }
                        else
                        {
                            resultBag.Remove(item.Key, item.Value);
                        }
                    }
                }
            }

            return resultBag;
        }

		#endregion

		#region Operator Overloads

		/// <summary>
		/// Operator + : Performs a union between two <see cref="Bag{T}"/>s.
		/// </summary>
        /// <param name="left">The left hand <see cref="Bag{T}"/>.</param>
        /// <param name="right">The right hand <see cref="Bag{T}"/>.</param>
        /// <returns>The union between <paramref name="left"/> and <paramref name="right"/>.</returns>        
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="OperatorAdd" lang="cs" title="The following example shows how to use the + operator overload."/>
		/// </example>
		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		[SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
		public static Bag<T> operator +(Bag<T> left, Bag<T> right)
		{
			return left.Union(right);
		}


		/// <summary>
		/// Operator - : Performs a difference operation between two Bags.
		/// </summary>
        /// <param name="left">The left hand <see cref="Bag{T}"/>.</param>
        /// <param name="right">The right hand <see cref="Bag{T}"/>.</param>
        /// <returns>The union between <paramref name="left"/> and <paramref name="right"/>.</returns>         
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="OperatorSubtract" lang="cs" title="The following example shows how to use the - operator overload."/>
		/// </example>        
		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		[SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
		public static Bag<T> operator -(Bag<T> left, Bag<T> right)
		{
			return left.Subtract(right);
		}


		/// <summary>
		/// Operator * : Performs a intersection between two Bags.
		/// </summary>
        /// <param name="left">The left hand <see cref="Bag{T}"/>.</param>
        /// <param name="right">The right hand <see cref="Bag{T}"/>.</param>
        /// <returns>The union between <paramref name="left"/> and <paramref name="right"/>.</returns>         
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="OperatorMultiply" lang="cs" title="The following example shows how to use the * operator overload."/>
		/// </example>           
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		public static Bag<T> operator *(Bag<T> left, Bag<T> right)
		{
			return left.Intersection(right);
		}


		/// <inheritdoc /> 
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="Item" lang="cs" title="The following example shows how to use the indexer method."/>
		/// </example>           
		public int this[T item]
		{
			get
			{
			    if (_data.TryGetValue(item, out var itemCount))
			    {
			        return itemCount;
			    }
			    
                return 0;
			}
		}

		#endregion

		#region ICollection<T> Members

	    /// <inheritdoc />
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="CopyTo" lang="cs" title="The following example shows how to use the CopyTo method."/>
		/// </example>   
		public void CopyTo(T[] array, int arrayIndex)
		{
			#region Validation


            Guard.ArgumentNotNull(array, "array");

			if ((array.Length - arrayIndex) < Count)
			{
                throw new ArgumentException(Constants.NotEnoughSpaceInTheTargetArray, "array");
			}

		    #endregion


			var counter = arrayIndex;

			foreach (var keyValuePair in _data)
			{
				var itemCount = keyValuePair.Value;
				var obj = keyValuePair.Key;

				for (var i = 0; i < itemCount; i++)
				{
					array.SetValue(obj, counter++);
				}
			}

		}


	    /// <inheritdoc />
	    /// <example>
	    /// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="Count" lang="cs" title="The following example shows how to use the IsFull property."/>
	    /// </example>           
	    public int Count { get; private set; }


	    /// <inheritdoc />
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="Add" lang="cs" title="The following example shows how to use the Add method."/>
		/// </example>
		public void Add(T item)
		{
			AddItem(item, 1);
		}


		/// <inheritdoc />
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="Remove" lang="cs" title="The following example shows how to use the Remove method."/>
		/// </example>        
		public bool Remove(T item)
		{
		    if (_data.TryGetValue(item, out var itemCount))
			{
                RemoveItem(item, 1, itemCount);
				return true;
			}
		    
            return false;
		}


		/// <inheritdoc />
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="Contains" lang="cs" title="The following example shows how to use the Contains method."/>
		/// </example>        
		public bool Contains(T item)
		{
			return _data.ContainsKey(item);
		}


		/// <inheritdoc />
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="GetEnumerator" lang="cs" title="The following example shows how to use the GetEnumerator method."/>
		/// </example>        
		public IEnumerator<T> GetEnumerator()
		{
            foreach (var item in _data)
            {
                yield return item.Key;
            }
        }


        IEnumerator<KeyValuePair<T, int>> IEnumerable<KeyValuePair<T, int>>.GetEnumerator()
        {
            foreach (var item in _data)
            {
                yield return item;
            }
        }


		/// <inheritdoc />
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="Clear" lang="cs" title="The following example shows how to use the Clear method."/>
		/// </example>         
		public void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Clears all the objects in this instance.
		/// </summary>
		/// <remarks>
		/// <b>Notes to Inheritors: </b>
		///  Derived classes can override this method to change the behavior of the <see cref="Clear"/> method.
		/// </remarks>
		protected virtual void ClearItems()
		{
			_data.Clear();
			Count = 0;
		}

		#endregion

		#region ICollection<T> Members

		/// <inheritdoc />
		/// <returns><c>false</c>.</returns>
		/// <remarks>Always returns <c>false</c> for <see cref="Bag{T}"/>.</remarks>
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="IsReadOnly" lang="cs" title="The following example shows how to use the IsReadOnly property."/>
		/// </example>     
		public bool IsReadOnly => false;

	    #endregion

		#region IEnumerable Members

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region IBag<T> Members

		/// <inheritdoc />
		IBag<T> IBag<T>.Intersection(IBag<T> bag)
		{
            return IntersectionInternal(bag);
		}

		/// <inheritdoc />
        IBag<T> IBag<T>.Subtract(IBag<T> bag)
        {
            return SubtractInternal(bag);
        }

		/// <inheritdoc />
        /// <exception cref="InvalidCastException"><paramref name="bag"/> is not a <see cref="Bag{T}"/>.</exception>
        IBag<T> IBag<T>.Union(IBag<T> bag)
        {
            return Union((Bag<T>)bag);
        }

		#endregion
        
		#region IEquatable<Bag<T>> Members

		/// <inheritdoc />
		/// <example>
		/// <code source="..\..\NGenerics.Examples\DataStructures\General\BagExamples.cs" region="Equals" lang="cs" title="The following example shows how to use the Equals method."/>
		/// </example>     
		public bool Equals(Bag<T> other)
		{
			if (Count != other?.Count)
			{
				return false;
			}
		    
            foreach (var item in _data)
            {
                if (!other.Contains(item.Key))
		        {
		            return false;
		        }
        
                if (other[item.Key] != item.Value)
                {
                    return false;
                }
            }

		    return true;
		}

		#endregion
	}
}