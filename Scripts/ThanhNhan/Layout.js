<script>
    /* ================== GIỎ HÀNG DEMO (UI) ================== */
    let _cart=[];                                              // Mảng giỏ tạm (demo)
    function addToCartDemo(id,name,price,img){                 // Thêm vào giỏ
      const f=_cart.find(x=>x.id===id);                        // Tìm sản phẩm theo id
      if(f){f.qty++} else {_cart.push({id,name,price,img,qty:1})} // Tăng SL hoặc thêm mới
      renderCart();                                            // Vẽ lại UI
      bootstrap.Offcanvas.getOrCreateInstance('#offcanvasCart').show(); // Mở giỏ
    }
    function changeQty(id,d){const it=_cart.find(x=>x.id===id);if(!it)return;it.qty=Math.max(1,it.qty+d);renderCart()} // +/- số lượng
    function removeFromCartDemo(id){_cart=_cart.filter(x=>x.id!==id);renderCart()} // Xóa item
    function renderCart(){                                     // Render danh sách + tổng
      const body=document.getElementById('cartBody'), count=document.getElementById('cartCount'), total=document.getElementById('cartTotal');
      if(_cart.length===0){body.innerHTML='<div class="text-muted">Giỏ hàng trống.</div>';count.innerText='0';total.innerText='0₫';return}
      let html='',sum=0;
      _cart.forEach(x=>{
        sum+=x.price*x.qty;
        html+=`<div class="d-flex align-items-center py-2 border-bottom">
          <img src="${x.img}" width="56" height="56" class="rounded me-2" style="object-fit:cover">
          <div class="flex-grow-1">
            <div class="d-flex justify-content-between">
              <strong>${x.name}</strong>
              <div>
                <button class="btn btn-sm btn-outline-secondary me-1" onclick="changeQty(${x.id},-1)">-</button>
                <button class="btn btn-sm btn-outline-secondary me-1" onclick="changeQty(${x.id},1)">+</button>
                <button class="btn btn-sm btn-link text-danger" onclick="removeFromCartDemo(${x.id})"><i class="bi bi-trash"></i></button>
              </div>
            </div>
            <small class="text-muted">${x.price.toLocaleString()}₫ x ${x.qty}</small>
          </div>
        </div>`;
      });
      body.innerHTML=html;
      count.innerText=_cart.reduce((a,b)=>a+b.qty,0);
      total.innerText=sum.toLocaleString()+'₫';
    }

    /* ================== SLIDERS ================== */
    // Promo (3 ô to)
    const promoSwiper=new Swiper('#promoSwiper',{
      loop:true, loopAdditionalSlides:4, speed:600,
      autoplay:{delay:3500, disableOnInteraction:false},
      spaceBetween:24, slidesPerView:1,
      breakpoints:{768:{slidesPerView:2}, 1200:{slidesPerView:3}},
      navigation:{nextEl:'#promoNext', prevEl:'#promoPrev'}
    });

    // Testimonials (1/2/3 cột)
    const testi=new Swiper('#testiSwiper',{
      loop:true, loopAdditionalSlides:2, speed:580,
      autoplay:{delay:3200, disableOnInteraction:false},
      spaceBetween:24, slidesPerView:1,
      breakpoints:{768:{slidesPerView:2}, 1200:{slidesPerView:3}},
      navigation:{nextEl:'#testiNext', prevEl:'#testiPrev'}
    });

    // Top Carousel (peek + chấm)
    const topSwiper=new Swiper('#topCarousel',{
      loop:true, speed:600,
      autoplay:{delay:3000, disableOnInteraction:false},
      slidesPerView:1.25, centeredSlides:true, spaceBetween:22,
      breakpoints:{768:{slidesPerView:1.6, spaceBetween:24}, 1200:{slidesPerView:2.2, spaceBetween:28}},
      pagination:{el:'.swiper-pagination', clickable:true}
    });

    /* ================== FLIP THẺ Ở CAROUSEL TOP ================== */
    document.querySelectorAll('#topCarousel .flip').forEach(card=>{
      const back=card.querySelector('.flip-back p');           // Phần mô tả mặt sau
      const desc=card.getAttribute('data-desc');               // Lấy mô tả từ data-desc
      if(desc && back){ back.textContent=desc }                // Gắn mô tả
      card.addEventListener('click', ()=>{                     // Click -> lật/đóng
        card.classList.toggle('is-flipped');
      });
    });

    /* ================== NAVBAR THU GỌN KHI LƯỚT ================== */
    const header=document.getElementById('appHeader');         // Header cần xử lý
    let lastY=window.scrollY;                                  // Vị trí Y trước đó
    let scrollTimer=null;                                      // Timer “dừng lướt”
    window.addEventListener('scroll',()=>{
      const y=window.scrollY;                                  // Vị trí hiện tại
      const goingDown=y>lastY;                                 // Đang cuộn xuống?
      if(goingDown && y>120){ header.classList.add('nav-hidden') }   // Ẩn hẳn
      else { header.classList.remove('nav-hidden'); header.classList.add('nav-compact') } // Thu gọn
      lastY=y;                                                 // Cập nhật mốc
      clearTimeout(scrollTimer);                               // Reset timer
      scrollTimer=setTimeout(()=>{ header.classList.remove('nav-hidden'); header.classList.add('nav-compact') },300); // Dừng -> thu gọn
    });
    header.addEventListener('mouseenter',()=>{ header.classList.remove('nav-hidden'); header.classList.remove('nav-compact') }); // Hover -> mở full
    header.addEventListener('mouseleave',()=>{ if(window.scrollY>120) header.classList.add('nav-compact') });                     // Rời -> thu gọn

    /* ================== DỮ LIỆU DEMO (đã sắp theo TOP) ================== */
    const dataBest=[
      {id:801, name:'Coca-Cola lon', price:10000, img:'https://picsum.photos/600/400?random=801', sold:14, stock:2, prog:85},
      {id:802, name:'Oreo Socola', price:15000, img:'https://picsum.photos/600/400?random=802', sold:12, stock:5, prog:70},
      {id:803, name:'Gạo ST25 Thơm', price:28000, img:'https://picsum.photos/600/400?random=803', sold:6,  stock:9, prog:40},
      {id:804, name:'Mì Hảo Hảo', price:3500,  img:'https://picsum.photos/600/400?random=804', sold:30, stock:20, prog:60},
      {id:805, name:'Sữa Vinamilk 1L', price:32000,img:'https://picsum.photos/600/400?random=805', sold:18, stock:6,  prog:75},
      {id:806, name:'Dầu ăn Tường An', price:48000,img:'https://picsum.photos/600/400?random=806', sold:9,  stock:11, prog:45},
      {id:807, name:'Nước suối 500ml', price:5000, img:'https://picsum.photos/600/400?random=807', sold:22, stock:15, prog:60},
      {id:808, name:'Trà xanh C2', price:9000,   img:'https://picsum.photos/600/400?random=808', sold:16, stock:10, prog:62},
      {id:809, name:'Bánh quy bơ', price:26000,  img:'https://picsum.photos/600/400?random=809', sold:11, stock:9,  prog:55},
    ];
    const dataFeat=[
      {id:901, name:'Cam Hữu Cơ', price:449000, old:545000, img:'https://picsum.photos/600/400?random=901', sold:10, stock:3, prog:78},
      {id:902, name:'Cà Rốt Hữu Cơ', price:560000, img:'https://picsum.photos/600/400?random=902', sold:8,  stock:5, prog:65},
      {id:903, name:'Hành Tím Hữu Cơ', price:54000, old:67000, img:'https://picsum.photos/600/400?random=903', sold:5,  stock:7, prog:42},
      {id:904, name:'Dưa Hấu Hữu Cơ', price:689000, img:'https://picsum.photos/600/400?random=904', sold:12, stock:4, prog:70},
      {id:905, name:'Nước Cam Hữu Cơ', price:240000, img:'https://picsum.photos/600/400?random=905', sold:9,  stock:6, prog:60},
      {id:906, name:'Thịt Bò Hữu Cơ', price:204500, old:200000, img:'https://picsum.photos/600/400?random=906', sold:10, stock:2, prog:83},
      {id:907, name:'Chuối Hữu Cơ', price:199000, old:239000, img:'https://picsum.photos/600/400?random=907', sold:13, stock:4, prog:76},
      {id:908, name:'Cải Tím Hữu Cơ', price:47000, img:'https://picsum.photos/600/400?random=908', sold:7,  stock:8, prog:47},
      {id:909, name:'Sữa Hữu Cơ', price:119000, img:'https://picsum.photos/600/400?random=909', sold:6,  stock:9, prog:40},
    ];

    /* ================== HUY HIỆU TOP THEO VỊ TRÍ (1..9) ================== */
    function rankBadge(index){
      const pos=index+1;                                       // Index 0 => TOP 1
      const cls = pos===1 ? 'rank-1' : pos===2 ? 'rank-2' : pos===3 ? 'rank-3' : 'rank-hot';
      const icon= pos<=3 ? 'bi-crown-fill' : 'bi-fire';
      return `<span class="rank-badge ${cls}"><span class="ico"><i class="bi ${icon}"></i></span><small>TOP ${pos}</small></span>`;
    }

    /* ================== TEMPLATE CARD CHUNG ================== */
    const cardTpl=(p,idx)=>`
      <div class="col-12 col-md-6 col-lg-4">
        <div class="card card-soft product-card h-100">
          ${rankBadge(idx)}
          <img class="w-100 product-img" src="${p.img}" alt="">
          <div class="p-3">
            <h5 class="mb-1">${p.name}</h5>
            <div class="price mb-2">
              ${p.price.toLocaleString()}₫
              ${p.old?` <span class="text-decoration-line-through text-muted ms-2">${p.old.toLocaleString()}₫</span>`:''}
            </div>
            <div class="stock-wrap mb-3">
              <div class="stock-bar"><span style="width:${p.prog}%"></span></div>
              <small class="text-muted">Có:${p.stock} • Bán:${p.sold}</small>
            </div>
            <div class="d-flex gap-2">
              <a class="btn btn-outline-success flex-fill" href="#"><i class="bi bi-eye me-1"></i> Xem</a>
              <button class="btn btn-brand flex-fill" onclick="addToCartDemo(${p.id},'${p.name}',${p.price},'${p.img}')"><i class="bi bi-bag-plus me-1"></i> Thêm</button>
            </div>
          </div>
        </div>
      </div>`;

    /* ================== RENDER GRID n PHẦN TỬ ================== */
    function renderGrid(containerId, data, n){
      const el=document.getElementById(containerId);
      el.innerHTML=data.slice(0,n).map((p,i)=>cardTpl(p,i)).join('');
    }

    // Khởi tạo 3 thẻ/khối
    renderGrid('gridBest', dataBest, 3);
    renderGrid('gridFeat', dataFeat, 3);

    /* ================== NÚT MORE/LESS (BÁN CHẠY) ================== */
    const btnMoreBest=document.getElementById('btnMoreBest');
    const btnLessBest=document.getElementById('btnLessBest');
    btnMoreBest.addEventListener('click',()=>{renderGrid('gridBest',dataBest,9);btnMoreBest.style.display='none';btnLessBest.style.display='inline-block';});
    btnLessBest.addEventListener('click',()=>{renderGrid('gridBest',dataBest,3);btnLessBest.style.display='none';btnMoreBest.style.display='inline-block';document.getElementById('best-sellers').scrollIntoView({behavior:'smooth'})});

    /* ================== NÚT MORE/LESS (NỔI BẬT) ================== */
    const btnMoreFeat=document.getElementById('btnMoreFeat');
    const btnLessFeat=document.getElementById('btnLessFeat');
    btnMoreFeat.addEventListener('click',()=>{renderGrid('gridFeat',dataFeat,9);btnMoreFeat.style.display='none';btnLessFeat.style.display='inline-block';});
    btnLessFeat.addEventListener('click',()=>{renderGrid('gridFeat',dataFeat,3);btnLessFeat.style.display='none';btnMoreFeat.style.display='inline-block';document.getElementById('featured').scrollIntoView({behavior:'smooth'})});

    /* ================== HIỆN THÊM / THU GỌN CHO CAROUSEL TOP ================== */
    // Danh sách slide thêm (demo). Sau này thay bằng data từ DB (đã sort).
    const topDataExtra=[
      {img:'https://picsum.photos/960/640?random=1011', tag:'TOP 5', color:'primary',  title:'Cam hữu cơ',      desc:'Cam vỏ mỏng, mọng nước. 49.000₫/kg'},
      {img:'https://picsum.photos/960/640?random=1012', tag:'TOP 6', color:'info',     title:'Dưa hấu',        desc:'Dưa ngọt giòn, vỏ mỏng. 35.000₫/kg'},
      {img:'https://picsum.photos/960/640?random=1013', tag:'TOP 7', color:'dark',     title:'Táo New Zealand',desc:'Táo giòn, ngọt thanh. 79.000₫/kg'},
      {img:'https://picsum.photos/960/640?random=1014', tag:'TOP 8', color:'secondary',title:'Nho đen',       desc:'Nho không hạt, đậm vị. 95.000₫/kg'}
    ];

    // Tạo DOM slide mới + đăng ký flip
    function makeTopSlide(d){
      const wrap=document.createElement('div');
      wrap.className='swiper-slide top-slide';
      wrap.innerHTML=`
        <div class="flip" data-desc="${d.desc}">
          <div class="flip-inner">
            <div class="flip-face flip-front">
              <img src="${d.img}" alt="${d.title}">
              <div class="flip-hint"><i class="bi bi-arrow-repeat"></i> Lật để xem</div>
            </div>
            <div class="flip-face flip-back">
              <span class="badge bg-${d.color} mb-2">${d.tag}</span>
              <h6>${d.title}</h6>
              <p>${d.desc}</p>
              <button class="btn btn-brand btn-sm mt-2"><i class="bi bi-bag-plus me-1"></i> Thêm giỏ</button>
            </div>
          </div>
        </div>`;
      wrap.querySelector('.flip').addEventListener('click',e=>{e.currentTarget.classList.toggle('is-flipped')});
      return wrap;
    }

    const btnMoreTop=document.getElementById('btnMoreTop');
    const btnLessTop=document.getElementById('btnLessTop');
    let _topAdded=false;

    // Hiện thêm slide vào cuối track
    btnMoreTop.addEventListener('click',()=>{
      if(_topAdded) return;
      _topAdded=true;
      const wrapper=document.querySelector('#topCarousel .swiper-wrapper');
      topDataExtra.forEach(d=>wrapper.appendChild(makeTopSlide(d)));
      topSwiper.update();                                      // Cập nhật Swiper
      btnMoreTop.style.display='none';
      btnLessTop.style.display='inline-block';
    });

    // Thu gọn: gỡ các slide đã thêm, cuộn về đầu khối
    btnLessTop.addEventListener('click',()=>{
      if(!_topAdded) return;
      const wrapper=document.querySelector('#topCarousel .swiper-wrapper');
      for(let i=0;i<topDataExtra.length;i++){
        const last=wrapper.lastElementChild; if(!last) break;
        wrapper.removeChild(last);
      }
      topSwiper.update();
      _topAdded=false;
      btnLessTop.style.display='none';
      btnMoreTop.style.display='inline-block';
      document.getElementById('top-products').scrollIntoView({behavior:'smooth', block:'start'});
    });

    /* ================== BACK-TO-TOP ================== */
    const backTop=document.getElementById('backTop');
    window.addEventListener('scroll',()=>{backTop.style.display=window.scrollY>400?'flex':'none'});
    backTop.addEventListener('click',()=>{window.scrollTo({top:0,behavior:'smooth'})});
  </script>